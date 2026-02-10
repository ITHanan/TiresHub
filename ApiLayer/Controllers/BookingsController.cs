using ApplicationLayer.Features.Bookings.Commands.AssignWarehouse;
using ApplicationLayer.Features.Bookings.Commands.CreateBooking;
using ApplicationLayer.Features.Bookings.Dtos;
using ApplicationLayer.Features.Bookings.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch;
using ApplicationLayer.Features.Bookings.Queries.GetBranchBookingsForManager;
using ApplicationLayer.Features.Bookings.Queries.GetMyBookings;
using DomainLayer.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        try
        {
            var result = await _mediator.Send(new CreateBookingCommand(dto), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Domain-level validation (e.g. branch full) -> return 400 with message
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
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
    [HttpGet("branch/manager/view")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<BookingListItemForManagerDto>>> ForManager(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBranchBookingsForManagerQuery(), ct);
        return Ok(result);
    }


    /// <summary>
    /// Get all bookings for the shop manager's assigned branch.
    /// </summary>
    [HttpGet("Get/All/Booking/For/The/Shop/Manager/Assigned/Branch")]
    public async Task<IActionResult> GetBookingsForBranch(CancellationToken ct)
    {
        var query = new GetBookingsForBranchQuery();
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Assign a warehouse to a booking (shop manager / owner only)
    /// </summary>
    [HttpPost("{bookingId:guid}/manager-assign-warehouse/{warehouseId:guid}")]
    [Authorize]
    public async Task<IActionResult> AssignWarehouse(Guid bookingId, Guid warehouseId, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdString))
            return Unauthorized("Invalid token.");

        var actorId = Guid.Parse(userIdString);

        var roleClaim = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(roleClaim))
            return Unauthorized("Invalid token.");

        if (!Enum.TryParse<UserRole>(roleClaim, out var actorRole))
            return Unauthorized("Invalid role.");

        var cmd = new AssignWarehouseCommand(
            ActorUserId: actorId,
            ActorRole: actorRole,
            BookingId: bookingId,
            WarehouseId: warehouseId
        );

        var result = await _mediator.Send(cmd, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Get details of a specific booking.
    /// </summary>
    [HttpGet("Get/Booking/Details/for/manager{bookingId}")]
    public async Task<IActionResult> GetBookingDetails(Guid bookingId, CancellationToken ct)
    {
        var query = new GetBookingDetailsQuery(bookingId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }


}

