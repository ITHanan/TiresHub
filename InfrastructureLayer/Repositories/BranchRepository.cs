using ApplicationLayer.Interfaces;
using DomainLayer.shops;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly AppDbContext _context;

        public BranchRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Branch branch, CancellationToken ct)
        {
            await _context.Branches.AddAsync(branch, ct);
        }

        public async Task<bool> BranchExistsAsync(Guid branchId, CancellationToken ct)
        {
            return await _context.Branches.AnyAsync(b => b.Id == branchId, ct);
        }

        public async Task<bool> BranchNameExistsAsync(Guid shopCompanyId, string name, CancellationToken ct)
        {
            var normalized = name.Trim().ToLower();
            return await _context.Branches.AnyAsync(b =>
                b.ShopCompanyId == shopCompanyId &&
                b.Name.ToLower() == normalized, ct);
        }

        public async Task<Guid?> GetCompanyIdByBranchIdAsync(Guid branchId, CancellationToken ct)
        {
            return await _context.Branches
                .Where(b => b.Id == branchId)
                .Select(b => (Guid?)b.ShopCompanyId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Branch?> GetByIdWithEmployeesAsync(Guid branchId, CancellationToken ct)
        {
            return await _context.Branches
                .Include(b => b.Employees)
                .FirstOrDefaultAsync(b => b.Id == branchId, ct);
        }
        public async Task<List<Branch>> GetByIdsWithEmployeesAsync(List<Guid> branchIds, CancellationToken ct)
        {
            return await _context.Branches
                .Include(b => b.Employees)
                .Where(b => branchIds.Contains(b.Id))
                .ToListAsync(ct);
        }

     
    }
}
