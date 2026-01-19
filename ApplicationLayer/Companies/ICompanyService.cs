using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Companies;

public interface ICompanyService
{
    Task<CompanyDto> RegisterCompanyAsync(RegisterCompanyRequest request, CancellationToken ct);
    Task<Guid?> GetMyCompanyIdAsync(CancellationToken ct);
}

