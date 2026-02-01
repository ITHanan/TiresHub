
namespace ApplicationLayer.Interfaces.Availability;

public interface IBranchAvailabilityRepository
{
    Task EnsureBranchHasCapacityAsync(Guid branchId, CancellationToken ct);
}
