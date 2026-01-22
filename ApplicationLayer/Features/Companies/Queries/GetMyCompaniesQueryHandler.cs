using ApplicationLayer.Features.Companies.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using MediatR;
namespace ApplicationLayer.Features.Companies.Queries.GetMyCompanies;

public class GetMyCompaniesQueryHandler
    : IRequestHandler<GetMyCompaniesQuery, List<CompanyDto>>

{
    private readonly ICompanyRepository _companyRepo;
    private readonly ICurrentUser _currentUser;

    public GetMyCompaniesQueryHandler(ICompanyRepository companyRepo, ICurrentUser currentUser)
    {
        _companyRepo = companyRepo;
        _currentUser = currentUser;
    }

    public async Task<List<CompanyDto>> Handle(GetMyCompaniesQuery request, CancellationToken ct)
    {
        var companies = await _companyRepo.GetMyCompaniesAsync(_currentUser.UserId, ct);

        var result = new List<CompanyDto>();
        foreach (var c in companies)
            result.Add(new CompanyDto(c.Name)); // du visar bara Name

        return result;
    }

}

