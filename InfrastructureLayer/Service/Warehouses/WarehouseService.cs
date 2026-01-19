using ApplicationLayer.Audit;
using ApplicationLayer.Companies;
using DomainLayer;
using DomainLayer.shops;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.Warehouses;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _db;
    private readonly ICompanyService _companyService;
    private readonly IAuditLogger _audit;

    public WarehouseService(AppDbContext db, ICompanyService companyService, IAuditLogger audit)
    {
        _db = db;
        _companyService = companyService;
        _audit = audit;
    }

    public async Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Warehouse name is required.");

        var companyId = await _companyService.GetMyCompanyIdAsync(ct)
            ?? throw new InvalidOperationException("Company not found.");

        var branchOk = await _db.Branches
            .AnyAsync(b => b.Id == request.BranchId && b.ShopCompanyId == companyId, ct);

        if (!branchOk)
            throw new UnauthorizedAccessException("You do not have permission to manage this branch.");

        // ✅ Use domain constructor
        var warehouse = new Warehouse(
            request.Name.Trim(),
            capacity: 0,
            branchId: request.BranchId
        );

        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "CreateWarehouse",
            "Warehouse",
            warehouse.Id.ToString(),
            new { warehouse.Name, warehouse.BranchId },
            ct
        );

        return warehouse.Id;
    }


public async Task UpdateAsync(Guid warehouseId, UpdateWarehouseRequest request, CancellationToken ct)
{
    var companyId = await _companyService.GetMyCompanyIdAsync(ct)
        ?? throw new InvalidOperationException("Company not found.");

    var warehouse = await _db.Warehouses
        .Include(w => w.Branch)
        .FirstOrDefaultAsync(w => w.Id == warehouseId, ct)
        ?? throw new KeyNotFoundException("Warehouse not found.");

    if (warehouse.Branch.ShopCompanyId != companyId)
        throw new UnauthorizedAccessException("You do not have permission to manage warehouses.");

    // ✅ Domain-sätt: inte setters
    warehouse.Rename(request.Name);

    if (request.IsActive) warehouse.Activate();
    else warehouse.Deactivate();

    await _db.SaveChangesAsync(ct);

    await _audit.LogAsync(
        "UpdateWarehouse",
        "Warehouse",
        warehouse.Id.ToString(),
        new { warehouse.Name, warehouse.IsActive },
        ct
    );
}

}
