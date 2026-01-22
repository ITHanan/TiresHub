
using DomainLayer.shops;

namespace ApplicationLayer.Interfaces
{
    public interface IWarehouseRepository
    {
        Task AddAsync(Warehouse warehouse, CancellationToken ct);
        Task<bool> ExistsAsync(Guid branchId, string name, CancellationToken ct);

        Task<Warehouse?> GetByIdAsync(Guid warehouseId, CancellationToken ct);
        Task<List<Warehouse>> ListByBranchIdAsync(Guid branchId, CancellationToken ct);

    }
}
