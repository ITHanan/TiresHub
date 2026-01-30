using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingDetails
{
    public record GetBookingDetailsQuery(Guid BookingId) : IRequest<BookingDetailsDto>;
}
