using ApplicationLayer.Features.Employees.Dtos;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Features.Employees.Queries
{
    public class GetBranchEmployeesQueryHandler
        : IRequestHandler<GetBranchEmployeesQuery, List<EmployeeDto>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepo;
        private readonly IBranchRepository _branchRepo;

        public GetBranchEmployeesQueryHandler(
            ICurrentUser currentUser,
            IUserRepository userRepo,
            IBranchRepository branchRepo)
        {
            _currentUser = currentUser;
            _userRepo = userRepo;
            _branchRepo = branchRepo;
        }

        public async Task<List<EmployeeDto>> Handle(GetBranchEmployeesQuery request, CancellationToken ct)
        {
            // 1. Validate authentication and authorization
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            if (_currentUser.Role != UserRole.ShopManager)
                throw new UnauthorizedAccessException("Only shop managers can view employees.");

            if (!_currentUser.BranchId.HasValue)
                throw new InvalidOperationException("Shop manager must be assigned to a branch.");

            // 2. Get employees for the manager's branch
            var branchId = _currentUser.BranchId.Value;
            var employees = await _userRepo.GetEmployeesByBranchIdAsync(branchId, ct);

            var branchName = await _branchRepo.GetBranchNameAsync(branchId, ct);

            // 3. Map to DTOs
            return employees.Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.UserEmail,
                Phone = e.Phone,
                BranchId = branchId,
                BranchName = branchName,
                IsActive = e.IsActive,
                Role = e.Role
            }).ToList();
        }
    }
}
