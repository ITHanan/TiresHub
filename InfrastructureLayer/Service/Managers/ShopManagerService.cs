// ApplicationLayer/Managers/ShopManagerService.cs
using ApplicationLayer.Audit;
using ApplicationLayer.Companies;
using DomainLayer;
using DomainLayer.Shops;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.Managers;

public class ShopManagerService : IShopManagerService
{
    private readonly AppDbContext _db;
    private readonly ICompanyService _companyService;
    private readonly IAuditLogger _audit;

    public ShopManagerService(AppDbContext db, ICompanyService companyService, IAuditLogger audit)
    {
        _db = db;
        _companyService = companyService;
        _audit = audit;
    }

    public async Task<Guid> CreateAsync(CreateShopManagerRequest request, CancellationToken ct)
    {
        var companyId = await _companyService.GetMyCompanyIdAsync(ct)
            ?? throw new InvalidOperationException("Company not found.");

        // Verify branches belong to company
        var validBranchIds = await _db.Branches
            .Where(b => b.ShopCompanyId == companyId && request.BranchIds.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(ct);

        if (validBranchIds.Count != request.BranchIds.Count)
            throw new UnauthorizedAccessException("One or more branches are invalid.");

        // ✅ Create via Domain constructor
        var manager = new ShopManager(
            request.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim()
        );

        _db.ShopManagers.Add(manager);

        foreach (var branchId in validBranchIds)
        {
            _db.BranchManagers.Add(new BranchManager(branchId, manager.Id));
        }

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "CreateShopManager",
            "ShopManager",
            manager.Id.ToString(),
            new { manager.Name, manager.Email, manager.Phone, BranchIds = validBranchIds },
            ct
        );

        return manager.Id;
    }

    public async Task SetActiveAsync(Guid managerId, bool isActive, CancellationToken ct)
    {
        var companyId = await _companyService.GetMyCompanyIdAsync(ct)
            ?? throw new InvalidOperationException("Company not found.");

        var manager = await _db.ShopManagers
            .Include(m => m.BranchManagers)
                .ThenInclude(bm => bm.Branch)
            .FirstOrDefaultAsync(m => m.Id == managerId, ct)
            ?? throw new KeyNotFoundException("Manager not found.");

        // säkerställ att manager hör till detta company via sina branches
        var belongsToCompany = manager.BranchManagers
            .Any(bm => bm.Branch.ShopCompanyId == companyId);

        if (!belongsToCompany)
            throw new UnauthorizedAccessException("Not allowed.");

        // ✅ använd domain-metoder
        if (isActive) manager.Activate();
        else manager.Deactivate();

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            isActive ? "ActivateShopManager" : "DeactivateShopManager",
            "ShopManager",
            manager.Id.ToString(),
            new { manager.IsActive },
            ct
        );
    }

}
