using ApplicationLayer.Interfaces;
using DomainLayer.shops;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly AppDbContext _db;

        public CompanyRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ShopCompany company, CancellationToken ct)
        {
            await _db.ShopCompanies.AddAsync(company, ct);
        }

        public Task<bool> ExistsAsync(string name, Guid ownerId, CancellationToken ct)
        {
            return _db.ShopCompanies.AnyAsync(
                x => x.Name == name && x.OwnerId == ownerId,
                ct);
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return _db.SaveChangesAsync(ct);
        }
        public Task<bool> OwnedByAsync(Guid companyId, Guid ownerId, CancellationToken ct)
        {
            return _db.ShopCompanies.AnyAsync(
                x => x.Id == companyId && x.OwnerId == ownerId,
                ct);
        }

    }
}
