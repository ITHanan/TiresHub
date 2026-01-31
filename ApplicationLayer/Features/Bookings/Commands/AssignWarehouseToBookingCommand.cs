using ApplicationLayer.Features.Bookings.Dtos;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Commands.AssignWarehouse
{
    public record AssignWarehouseToBookingCommand(
        Guid BookingId,
        Guid WarehouseId
    ) : IRequest<BookingDto>;
}
