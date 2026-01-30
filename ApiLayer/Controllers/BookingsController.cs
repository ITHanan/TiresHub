using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all bookings for the shop manager's assigned branch.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBookingsForBranch(CancellationToken ct)
        {
            var query = new GetBookingsForBranchQuery();
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get details of a specific booking.
        /// </summary>
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingDetails(Guid bookingId, CancellationToken ct)
        {
            var query = new GetBookingDetailsQuery(bookingId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
    }
}
