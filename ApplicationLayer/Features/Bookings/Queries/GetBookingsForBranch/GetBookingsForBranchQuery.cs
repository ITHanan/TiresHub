using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch
{
    public record GetBookingsForBranchQuery : IRequest<List<BookingSummaryDto>>;
}
