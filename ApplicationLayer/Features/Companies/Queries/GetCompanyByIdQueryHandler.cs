using MediatR;
using ApplicationLayer.Features.Companies.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;

namespace ApplicationLayer.Features.Companies.Queries.GetCompanyById;


public class GetCompanyByIdQueryHandler
    : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyRepository _companyRepo;

    public GetCompanyByIdQueryHandler(
        ICurrentUser currentUser,
        ICompanyRepository companyRepo)
    {
        _currentUser = currentUser;
        _companyRepo = companyRepo;
    }

    public async Task<CompanyDto?> Handle(
        GetCompanyByIdQuery request,
        CancellationToken ct)
    {
        var company = await _companyRepo.GetByIdAsync(request.Id, ct);
        if (company is null) return null;

        // Säkerhet: användare får bara se sitt eget företag
        if (company.OwnerId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        return new CompanyDto(
            
            company.Name
        );
    }
}
