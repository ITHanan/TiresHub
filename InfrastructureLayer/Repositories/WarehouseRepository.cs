using ApplicationLayer.Interfaces;
using DomainLayer.shops;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(Warehouse warehouse, CancellationToken ct)
        {
            await _context.Warehouses.AddAsync(warehouse, ct);
        }

        public async Task<bool> ExistsAsync(Guid branchId, string name, CancellationToken ct)
        {
            var normalized = name.Trim().ToLower();

            return await _context.Warehouses.AnyAsync(w =>
                w.BranchId == branchId &&
                w.Name.ToLower() == normalized,
                ct);
        }
        public async Task<Warehouse?> GetByIdAsync(Guid warehouseId, CancellationToken ct)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == warehouseId, ct);
        }
        public async Task<List<Warehouse>> ListByBranchIdAsync(Guid branchId, CancellationToken ct)
        {
            return await _context.Warehouses
                .Where(w => w.BranchId == branchId)
                .OrderBy(w => w.Name)
                .ToListAsync(ct);
        }

    }
}
