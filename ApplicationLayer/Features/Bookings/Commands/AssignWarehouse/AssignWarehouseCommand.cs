using DomainLayer.Common;
using MediatR;
using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.Commands.AssignWarehouse
{
    public record AssignWarehouseCommand(
        Guid ActorUserId,
        UserRole ActorRole,
        Guid BookingId,
        Guid WarehouseId
    ) : IRequest<OperationResult<Unit>>;
}
