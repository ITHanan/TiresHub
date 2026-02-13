using ApplicationLayer.Features.Employees.Dtos;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using DomainLayer.Users;
using MediatR;

namespace ApplicationLayer.Features.Employees.Commands
{
    public class CreateEmployeeCommandHandler
        : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IAuditRepository _auditRepo;

        public CreateEmployeeCommandHandler(
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

        public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken ct)
        {
            // 1. Validate authentication and authorization
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            if (_currentUser.Role != UserRole.ShopManager)
                throw new UnauthorizedAccessException("Only shop managers can create employee accounts.");

            if (!_currentUser.BranchId.HasValue)
                throw new InvalidOperationException("Shop manager must be assigned to a branch.");

            // 2. Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Name is required.");

            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
            var hasPhone = !string.IsNullOrWhiteSpace(request.Phone);

            if (!hasEmail && !hasPhone)
                throw new InvalidOperationException("Email or phone is required.");

            // For now, require email (similar to manager registration)
            if (!hasEmail)
                throw new InvalidOperationException("Email is required right now (phone-only not supported yet).");

            var email = request.Email!.Trim().ToLowerInvariant();

            // 3. Validate branch exists and manager has access
            var branchId = _currentUser.BranchId.Value;
            var branchExists = await _branchRepo.BranchExistsAsync(branchId, ct);
            if (!branchExists)
                throw new InvalidOperationException("Branch not found.");

            var hasAccess = await _branchRepo.ManagerHasAccessToBranchAsync(_currentUser.UserId, branchId, ct);
            if (!hasAccess)
                throw new UnauthorizedAccessException("You do not have permission to manage this branch.");

            // 4. Check if user already exists
            var existingUser = await _userRepo.GetByEmailAsync(email);
            if (existingUser != null)
            {
                // User exists - check if they can be assigned as employee
                if (existingUser.Role != UserRole.Employee)
                    throw new InvalidOperationException($"User exists with role {existingUser.Role}. Cannot assign as employee.");

                if (existingUser.BranchId.HasValue && existingUser.BranchId.Value != branchId)
                    throw new InvalidOperationException("Employee is already assigned to another branch.");

                if (!existingUser.BranchId.HasValue)
                {
                    // Assign to branch
                    existingUser.AssignBranch(branchId);
                    await _userRepo.SaveChangesAsync();

                    // Audit log
                    await _auditRepo.LogAsync(
                        _currentUser.UserId,
                        "EMPLOYEE_ASSIGNED",
                        "User",
                        existingUser.Id,
                        true,
                        metadata: $"Assigned to branch {branchId}"
                    );
                }

                var branchName = await _branchRepo.GetBranchNameAsync(branchId, ct);

                return new EmployeeDto
                {
                    Id = existingUser.Id,
                    Name = existingUser.Name,
                    Email = existingUser.UserEmail,
                    Phone = existingUser.Phone,
                    BranchId = branchId,
                    BranchName = branchName,
                    IsActive = existingUser.IsActive,
                    Role = existingUser.Role
                };
            }

            // 5. Create new employee
            var employee = new User(
                name: request.Name.Trim(),
                email: email,
                phone: request.Phone?.Trim(),
                role: UserRole.Employee
            );

            // 6. Assign to manager's branch (immutable after creation)
            employee.AssignBranch(branchId);

            await _userRepo.AddAsync(employee);
            await _userRepo.SaveChangesAsync();

            // 7. Audit log
            await _auditRepo.LogAsync(
                _currentUser.UserId,
                "EMPLOYEE_CREATED",
                "User",
                employee.Id,
                true,
                metadata: $"Created employee for branch {branchId}"
            );

            var branchNameResult = await _branchRepo.GetBranchNameAsync(branchId, ct);

            return new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.UserEmail,
                Phone = employee.Phone,
                BranchId = branchId,
                BranchName = branchNameResult,
                IsActive = employee.IsActive,
                Role = employee.Role
            };
        }
    }
}
