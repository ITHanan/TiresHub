using ApplicationLayer.Features.Bookings.Dtos;
using ApplicationLayer.Features.Bookings.DTOs;
using MediatR;
using System;

namespace ApplicationLayer.Features.Bookings.Queries.GetBranchBookingsForManager;

public sealed record GetBranchBookingsForManagerQuery() : IRequest<IReadOnlyList<BookingListItemForManagerDto>>;
