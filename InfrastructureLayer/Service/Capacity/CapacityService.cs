// ApplicationLayer/Capacity/CapacityService.cs
using ApplicationLayer.Audit;
using ApplicationLayer.Companies;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.Capacity;

public class CapacityService : ICapacityService
{
    private readonly AppDbContext _db;
    private readonly ICompanyService _companyService;
    private readonly IAuditLogger _audit;

    public CapacityService(AppDbContext db, ICompanyService companyService, IAuditLogger audit)
    {
        _db = db;
        _companyService = companyService;
        _audit = audit;
    }
    public async Task UpdateWarehouseCapacityAsync(Guid warehouseId, UpdateCapacityRequest request, CancellationToken ct)
    {
        if (request.Capacity < 0)
            throw new ArgumentException("Storage capacity must be a positive number.");

        var companyId = await _companyService.GetMyCompanyIdAsync(ct)
            ?? throw new InvalidOperationException("Company not found.");

        var warehouse = await _db.Warehouses
            .Include(w => w.Branch)
            .FirstOrDefaultAsync(w => w.Id == warehouseId, ct)
            ?? throw new KeyNotFoundException("Warehouse not found.");

        if (warehouse.Branch.ShopCompanyId != companyId)
            throw new UnauthorizedAccessException("You do not have permission.");

        // Domain rule (A2 handled inside SetCapacity)
        warehouse.SetCapacity(request.Capacity, request.ForceIfBelowUsage);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "UpdateWarehouseCapacity",
            "Warehouse",
            warehouse.Id.ToString(),
            new { warehouse.Capacity, warehouse.CurrentUsage, IsFull = warehouse.IsFull() },
            ct
        );
    }

}
