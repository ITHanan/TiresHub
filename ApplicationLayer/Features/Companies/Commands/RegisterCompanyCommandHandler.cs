using ApplicationLayer.Features.Companies.Commands;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.shops;
using MediatR;

public class RegisterCompanyCommandHandler
    : IRequestHandler<RegisterCompanyCommand, Guid>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepo;
    private readonly ICompanyRepository _companyRepo;

    public RegisterCompanyCommandHandler(
        ICurrentUser currentUser,
        IUserRepository userRepo,
        ICompanyRepository companyRepo)
    {
        _currentUser = currentUser;
        _userRepo = userRepo;
        _companyRepo = companyRepo;
    }

    public async Task<Guid> Handle(RegisterCompanyCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        var company = new ShopCompany(request.Name, user.Id);

        await _companyRepo.AddAsync(company, ct);
        await _companyRepo.SaveChangesAsync(ct);

        return company.Id;
    }
}
