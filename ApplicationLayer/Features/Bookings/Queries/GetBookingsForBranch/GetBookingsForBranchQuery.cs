using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch
{
    public record GetBookingsForBranchQuery : IRequest<List<BookingSummaryDto>>;
}
