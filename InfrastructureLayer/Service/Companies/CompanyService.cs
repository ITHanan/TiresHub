using ApplicationLayer.Audit;
using ApplicationLayer.Common;
using ApplicationLayer.Common.Mappings;
using ApplicationLayer.Companies;
using DomainLayer.shops;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Service.Companies;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public CompanyService(AppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public Guid CompanyId { get; }

    public async Task<Guid?> GetMyCompanyIdAsync(CancellationToken ct)
    {
        var ownerId = _currentUser.UserId;

        return await _db.ShopCompanies
            .AsNoTracking()
            .Where(c => c.OwnerId == ownerId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);
    }



    public async Task<CompanyDto> RegisterCompanyAsync(
        RegisterCompanyRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Company name is required.");

        var ownerId = _currentUser.UserId;

        var company = new ShopCompany(
            request.Name.Trim(),
            ownerId
        );

        _db.ShopCompanies.Add(company);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "RegisterCompany",
            "ShopCompany",
            company.Id.ToString(),
            new { company.Name },
            ct
        );

        return new CompanyDto(company.Id, company.Name);
    }

  
}
