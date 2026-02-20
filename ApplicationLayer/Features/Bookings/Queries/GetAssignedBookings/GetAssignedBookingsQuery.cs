using MediatR;
using System.Collections.Generic;

namespace ApplicationLayer.Features.Bookings.Queries.GetAssignedBookings;

public sealed record GetAssignedBookingsQuery() : IRequest<List<AssignedBookingDto>>;