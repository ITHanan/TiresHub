using ApplicationLayer.Features.Managers.Dtos;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using DomainLayer.Users;
using MediatR;

namespace ApplicationLayer.Managers.Commands.RegisterShopManager
{
    public class RegisterShopManagerCommandHandler
        : IRequestHandler<RegisterShopManagerCommand, ShopManagerDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICompanyRepository _companyRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IUserRepository _userRepo;

        public RegisterShopManagerCommandHandler(
            ICurrentUser currentUser,
            ICompanyRepository companyRepo,
            IBranchRepository branchRepo,
            IUserRepository userRepo)
        {
            _currentUser = currentUser;
            _companyRepo = companyRepo;
            _branchRepo = branchRepo;
            _userRepo = userRepo;
        }
        public async Task<ShopManagerDto> Handle(RegisterShopManagerCommand request, CancellationToken ct)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Name is required.");

            if (request.BranchId is null || request.BranchId.Count == 0)
                throw new InvalidOperationException("At least one branch must be assigned.");

            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
            var hasPhone = !string.IsNullOrWhiteSpace(request.Phone);

            if (!hasEmail && !hasPhone)
                throw new InvalidOperationException("Email or phone is required.");

            // (Starta enkelt: kräv email om du inte har phone-lookup än)
            if (!hasEmail)
                throw new InvalidOperationException("Email is required right now (phone-only not supported yet).");

            var email = request.Email!.Trim().ToLowerInvariant();

            // 1) Hämta alla branches inkl employees
            var branches = await _branchRepo.GetByIdsWithEmployeesAsync(request.BranchId, ct);
            if (branches.Count != request.BranchId.Count)
                throw new InvalidOperationException("One or more branches not found.");

            // 2) Ownership-check för varje branch
            foreach (var b in branches)
            {
                var owned = await _companyRepo.OwnedByAsync(b.ShopCompanyId, _currentUser.UserId, ct);
                if (!owned)
                    throw new UnauthorizedAccessException("You do not have permission to manage one of the branches.");
            }

            // 3) Hämta eller skapa user
            var user = await _userRepo.GetByEmailAsync(email);

            if (user is null)
            {
                user = new User(
                    name: request.Name.Trim(),
                    email: email,
                    phone: request.Phone,
                    role: UserRole.ShopManager
                );

                await _userRepo.AddAsync(user);
            }
            else
            {
                if (user.Role != UserRole.ShopManager)
                    throw new InvalidOperationException("User exists but is not a manager.");
            }

            // 4) Koppla user till alla branches (utan duplicates)
            foreach (var b in branches)
            {
                if (!b.Employees.Any(e => e.Id == user.Id))
                    b.Employees.Add(user);
            }

            await _userRepo.SaveChangesAsync();

            // returnera nån “primär” branch (för DTOn du har idag)
            var first = branches[0];

            return new ShopManagerDto
            {
                UserId = user.Id,
                ShopCompanyId = first.ShopCompanyId,
                BranchId = first.Id,
                Email = user.UserEmail,
                Phone = user.Phone,
                Role = user.Role
            };
        }


    }
}
