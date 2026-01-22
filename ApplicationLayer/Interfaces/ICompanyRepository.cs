using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.shops;
namespace ApplicationLayer.Interfaces
{
    public interface ICompanyRepository
    {
        Task AddAsync(ShopCompany company, CancellationToken ct);
        Task<bool> ExistsAsync(string name, Guid ownerId, CancellationToken ct);
        Task<ShopCompany?> GetByIdAsync(Guid id, CancellationToken ct);

        Task<bool> OwnedByAsync(Guid companyId, Guid ownerId, CancellationToken ct);

      
        Task SaveChangesAsync(CancellationToken ct);
        Task<List<ShopCompany>> GetMyCompaniesAsync(Guid ownerId, CancellationToken ct);


    }
}
