using DomainLayer.Common;
using MediatR;
using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.Commands.AssignEmployee
{
    public record AssignEmployeeCommand(
        Guid ActorUserId,
        UserRole ActorRole,
        Guid? ActorBranchId,
        Guid BookingId,
        Guid EmployeeId
    ) : IRequest<OperationResult<Unit>>;
}
