using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Auditing;
using DomainLayer.Common;
using MediatR;

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

            // Business logic:
            // 1. Validate actor's permissions (shop manager of the branch only hem can assign )


            if (request.ActorRole != DomainLayer.Enums.UserRole.ShopManager)
            {
                await _auditRepo.LogAsync(

                    request.ActorUserId,
                    AuditActions.UnauthorizedEmployeeAssignment,
                    nameof(Bookings),
                    request.BookingId,
                    false,
                    $"Unauthorized attempt to assign employee {request.EmployeeId} to booking {request.BookingId} by user {request.ActorUserId} with role {request.ActorRole}",
                    null


                    );
                return OperationResult<Unit>.Failure("Unauthorized. Only shop managers can assign employees to bookings.");
            }
                //BE-67 Validate that the shop manager is assigning within their own branch
                var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
                if (booking == null)
                {
                    return OperationResult<Unit>.Failure("Booking not found.");
                }

                //BE-69 Validate that the employee belongs to the same branch as the booking

                if (booking.BranchId != request.ActorBranchId)
                {
                    await _auditRepo.LogAsync(
                        request.ActorUserId,
                        AuditActions.UnauthorizedEmployeeAssignment,
                        nameof(Bookings),
                        request.BookingId,
                        false,
                        $"Unauthorized attempt to assign employee {request.EmployeeId} to booking {request.BookingId} in branch {booking.BranchId} by user {request.ActorUserId} with role {request.ActorRole}",
                        null
                        );
                    return OperationResult<Unit>.Failure("Unauthorized. You can only assign employees to bookings within your own branch.");

                }

                //BE-67 validate employee exists and belongs to the same branch

                var employee = await _userRepo.GetByIdAsync(request.EmployeeId);
                if (employee.Role != DomainLayer.Enums.UserRole.Employee)

                    return OperationResult<Unit>.Failure("The specified user is not an employee.");

                //BE-69 validate employee is active 
                if (!employee.IsActive)
                {
                    return OperationResult<Unit>.Failure("The specified employee is not active.");
                }

                //BE-69 validate employee belongs to the same branch as the booking

                if (employee.BranchId != booking.BranchId)
                {
                    await _auditRepo.LogAsync(
                        request.ActorUserId,
                        AuditActions.EmployeeAssignmentFailed,
                        nameof(Bookings),
                        request.BookingId,
                        false,
                        $"Unauthorized attempt to assign employee {request.EmployeeId} from branch {employee.BranchId} to booking {request.BookingId} in branch {booking.BranchId} by user {request.ActorUserId} with role {request.ActorRole}",
                        null
                        );
                    return OperationResult<Unit>.Failure("Unauthorized. You can only assign employees from the same branch as the booking.");
                }



                //BE-68 validate Detect if booking already has an assigned employee and prevent overwriting without unassigning first

                var existingEmployeeId = booking.AssignedEmployeeId;
                var isReassignment = existingEmployeeId.HasValue && existingEmployeeId.Value != request.EmployeeId;

                booking.AssignEmployee(request.EmployeeId);

                //persist changes
                await _companyRepo.SaveChangesAsync(cancellationToken);


                //BE-68 log if this is a reassignment

                var auditAction = isReassignment ? AuditActions.EmployeeReassigned : AuditActions.EmployeeAssigned;
                var metadata = isReassignment ? $"Reassigned from employee {existingEmployeeId} to {request.EmployeeId}" : $"Assigned employee {request.EmployeeId}";

                await _auditRepo.LogAsync
                 (
                    request.ActorUserId,
                    auditAction,
                    nameof(Bookings),
                    booking.Id,
                    true,
                    null,
                    metadata
                 );


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