using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingDetails
{
    public record GetBookingDetailsQuery(Guid BookingId) : IRequest<BookingDetailsDto>;
}
