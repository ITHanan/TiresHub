using ApplicationLayer.Audit;
using ApplicationLayer.Branches;
using ApplicationLayer.Companies;
using DomainLayer.shops;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Service.Branches;

public class BranchService : IBranchService
{
    private readonly AppDbContext _db;
    private readonly ICompanyService _companyService;
    private readonly IAuditLogger _audit;

    public BranchService(
        AppDbContext db,
        ICompanyService companyService,
        IAuditLogger audit)
    {
        _db = db;
        _companyService = companyService;
        _audit = audit;
    }

    public async Task<BranchDto> CreateBranchAsync(
        CreateBranchRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.Address))
            throw new ArgumentException("Branch name, city, and address are required.");

        var companyId = await _companyService.GetMyCompanyIdAsync(ct)
            ?? throw new InvalidOperationException("Company not found.");

        var branch = new Branch(
            request.Name.Trim(),
            request.City.Trim(),
            request.Address.Trim(),
            companyId
        );

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "CreateBranch",
            "Branch",
            branch.Id.ToString(),
            new { branch.Name, branch.City },
            ct
        );

        // ✅ Returnera DTO 
        return new BranchDto(
            branch.Id,
            branch.Name,
            branch.City,
            branch.Address,
            branch.IsActive
        );
    }


    public async Task<List<BranchDto>> GetMyBranchesAsync(CancellationToken ct)
    {
        var companyId = await _companyService.GetMyCompanyIdAsync(ct)
            ?? throw new InvalidOperationException("Company not found.");

        return await _db.Branches
    .AsNoTracking()
    .Where(b => b.ShopCompanyId == companyId)
    .OrderBy(b => b.Name)
    .Select(b => new BranchDto(
        b.Id,
        b.Name,
        b.City,
        b.Address,
        b.IsActive
    ))
    .ToListAsync(ct);

    }

}
