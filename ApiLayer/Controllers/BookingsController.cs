
using ApplicationLayer.Features.Bookings.Commands.CreateBooking;
using ApplicationLayer.Features.Bookings.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApplicationLayer.Features.Bookings.Queries.GetBranchBookingsForManager;
using ApplicationLayer.Features.Bookings.Queries.GetMyBookings;
using Microsoft.AspNetCore.Authorization;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BookingsController(IMediator mediator) => _mediator = mediator;

    // UC-10/11/12: Create booking + confirmation
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingConfirmationDto>> Create([FromBody] CreateBookingRequestDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateBookingCommand(dto), ct);
        return Ok(result);
    }

    // UC-12: owner dashboard
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<BookingListItemDto>>> Mine(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyBookingsQuery(), ct);
        return Ok(result);
    }

    // UC-10: manager view
    [HttpGet("branch")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<BookingListItemDto>>> ForManager(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBranchBookingsForManagerQuery(), ct);
        return Ok(result);
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

