using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ApplicationLayer.Features.Bookings.DTOs;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetMyBookings;

public sealed record GetMyBookingsQuery() : IRequest<IReadOnlyList<BookingListItemDto>>;
