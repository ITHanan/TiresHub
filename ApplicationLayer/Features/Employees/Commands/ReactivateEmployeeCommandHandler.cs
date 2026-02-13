using ApplicationLayer.Features.Employees.Dtos;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Features.Employees.Commands
{
    public class ReactivateEmployeeCommandHandler
        : IRequestHandler<ReactivateEmployeeCommand, EmployeeDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IAuditRepository _auditRepo;

        public ReactivateEmployeeCommandHandler(
            ICurrentUser currentUser,
            IUserRepository userRepo,
            IBranchRepository branchRepo,
            IAuditRepository auditRepo)
        {
            _currentUser = currentUser;
            _userRepo = userRepo;
            _branchRepo = branchRepo;
            _auditRepo = auditRepo;
        }

        public async Task<EmployeeDto> Handle(ReactivateEmployeeCommand request, CancellationToken ct)
        {
            // 1. Validate authentication and authorization
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            if (_currentUser.Role != UserRole.ShopManager)
                throw new UnauthorizedAccessException("Only shop managers can reactivate employees.");

            if (!_currentUser.BranchId.HasValue)
                throw new InvalidOperationException("Shop manager must be assigned to a branch.");

            // 2. Get employee
            var employee = await _userRepo.GetByIdAsync(request.EmployeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.Role != UserRole.Employee)
                throw new InvalidOperationException("User is not an employee.");

            // 3. Validate branch access
            if (!employee.BranchId.HasValue)
                throw new InvalidOperationException("Employee is not assigned to a branch.");

            if (employee.BranchId.Value != _currentUser.BranchId.Value)
                throw new UnauthorizedAccessException("You can only manage employees in your own branch.");

            // 4. Reactivate
            employee.Activate();
            await _userRepo.SaveChangesAsync();

            // 5. Audit log
            await _auditRepo.LogAsync(
                _currentUser.UserId,
                "EMPLOYEE_REACTIVATED",
                "User",
                employee.Id,
                true,
                metadata: $"Employee reactivated in branch {employee.BranchId}"
            );

            var branchName = await _branchRepo.GetBranchNameAsync(employee.BranchId.Value, ct);

            return new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.UserEmail,
                Phone = employee.Phone,
                BranchId = employee.BranchId.Value,
                BranchName = branchName,
                IsActive = employee.IsActive,
                Role = employee.Role
            };
        }
    }
}
