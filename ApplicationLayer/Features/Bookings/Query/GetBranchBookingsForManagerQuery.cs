using System;
using ApplicationLayer.Features.Bookings.DTOs;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetBranchBookingsForManager;

public sealed record GetBranchBookingsForManagerQuery() : IRequest<IReadOnlyList<BookingListItemDto>>;
