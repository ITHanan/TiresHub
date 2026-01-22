using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DomainLayer.shops;
public interface IBranchRepository
{
    Task AddAsync(Branch branch, CancellationToken ct);

    Task<bool> BranchExistsAsync(Guid branchId, CancellationToken ct);
    Task<bool> BranchNameExistsAsync(Guid shopCompanyId, string name, CancellationToken ct);

    Task<Guid?> GetCompanyIdByBranchIdAsync(Guid branchId, CancellationToken ct);
    Task<Branch?> GetByIdWithEmployeesAsync(Guid branchId, CancellationToken ct);
  
    Task<List<Branch>> GetByIdsWithEmployeesAsync(List<Guid> branchIds, CancellationToken ct);


}
