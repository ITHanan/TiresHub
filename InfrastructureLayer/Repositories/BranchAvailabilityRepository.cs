
using ApplicationLayer.Interfaces.Availability;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Services;

public sealed class BranchAvailabilityRepository : IBranchAvailabilityRepository
{
    private readonly AppDbContext _db;
    public BranchAvailabilityRepository(AppDbContext db) => _db = db;

    public async Task EnsureBranchHasCapacityAsync(Guid branchId, CancellationToken ct)
    {
        // Anpassa efter din Warehouse-modell:
        // Ex: w.Capacity och w.UsedCapacity
        var ok = await _db.Warehouses.AnyAsync(w =>
    w.BranchId == branchId &&
    w.IsActive &&
    w.CurrentUsage < w.Capacity, ct);

        if (!ok)
            throw new InvalidOperationException("Selected branch is currently unavailable for new bookings.");

    }
}
