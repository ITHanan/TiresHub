using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationLayer.Branches;

public interface IBranchService
{
    Task<BranchDto> CreateBranchAsync(CreateBranchRequest request, CancellationToken ct);
    Task<List<BranchDto>> GetMyBranchesAsync(CancellationToken ct);
}




