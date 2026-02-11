using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.Common;
using DomainLayer.Auditing;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Bookings.Commands.AssignEmployee
{
    public class AssignEmployeeCommandHandler : IRequestHandler<AssignEmployeeCommand, OperationResult<Unit>>
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly IAuditRepository _auditRepo;

        public AssignEmployeeCommandHandler(
            IBookingRepository bookingRepo,
            IUserRepository userRepo,
            ICompanyRepository companyRepo,
            IAuditRepository auditRepo)
        {
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
            _auditRepo = auditRepo;
        }

        public async Task<OperationResult<Unit>> Handle(AssignEmployeeCommand request, CancellationToken cancellationToken)
        {
            // SEC-20: Authorization - only shop manager can assign
            if (request.ActorRole != UserRole.ShopManager)
            {
                await _auditRepo.LogAsync(
                    request.ActorUserId,
                    AuditActions.UnauthorizedEmployeeAssignment,
                    nameof(Booking),
                    request.BookingId,
                    false,
                    $"User with role {request.ActorRole} attempted to assign employee",
                    null);
                return OperationResult<Unit>.Failure("Unauthorized. Only shop managers can assign employees.");
            }

            // BE-67: Validate booking exists
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
                return OperationResult<Unit>.Failure("Booking not found.");

            // BE-69: Validate booking belongs to manager's branch
            if (booking.BranchId != request.ActorBranchId)
            {
                await _auditRepo.LogAsync(
                    request.ActorUserId,
                    AuditActions.UnauthorizedEmployeeAssignment,
                    nameof(Booking),
                    request.BookingId,
                    false,
                    "Attempted to assign employee to booking from different branch",
                    null);
                return OperationResult<Unit>.Failure("You can only assign employees to bookings from your branch.");
            }

            // BE-67: Validate employee exists
            var employee = await _userRepo.GetByIdAsync(request.EmployeeId);
            if (employee is null)
                return OperationResult<Unit>.Failure("Employee not found.");

            // BE-67: Validate employee role
            if (employee.Role != UserRole.Employee)
                return OperationResult<Unit>.Failure("Selected user is not an employee.");

            // BE-67: Validate employee is active
            if (!employee.IsActive)
                return OperationResult<Unit>.Failure("Cannot assign inactive employee.");

            // BE-69: Validate employee belongs to same branch
            if (employee.BranchId != booking.BranchId)
            {
                await _auditRepo.LogAsync(
                    request.ActorUserId,
                    AuditActions.EmployeeAssignmentFailed,
                    nameof(Booking),
                    request.BookingId,
                    false,
                    "Employee belongs to different branch",
                    null);
                return OperationResult<Unit>.Failure("You can only assign employees from your branch.");
            }

            // BE-68: Detect if booking already has assigned employee (reassignment)
            var previousEmployeeId = booking.AssignedEmployeeId;
            var isReassignment = previousEmployeeId.HasValue;

            // BE-67: Assign employee
            booking.AssignEmployee(request.EmployeeId);

            // Persist changes
            await _companyRepo.SaveChangesAsync(cancellationToken);

            // BE-67/BE-68: Audit log assignment or reassignment
            var auditAction = isReassignment ? AuditActions.EmployeeReassigned : AuditActions.EmployeeAssigned;
            var metadata = isReassignment ? $"Previous employee: {previousEmployeeId}" : null;

            await _auditRepo.LogAsync(
                request.ActorUserId,
                auditAction,
                nameof(Booking),
                booking.Id,
                true,
                null,
                metadata);

            // BE-67: Trigger employee notification (mocked)
            // In production, this would send a notification to the employee
            // For now, this is a no-op placeholder
            await NotifyEmployeeAsync(request.EmployeeId, booking.Id, cancellationToken);

            return OperationResult<Unit>.Success(Unit.Value);
        }

        // BE-67: Mock notification method
        private Task NotifyEmployeeAsync(Guid employeeId, Guid bookingId, CancellationToken cancellationToken)
        {
            // Mock implementation - in production this would:
            // - Send email notification
            // - Push notification to mobile app
            // - Create in-app notification
            return Task.CompletedTask;
        }
    }
}
