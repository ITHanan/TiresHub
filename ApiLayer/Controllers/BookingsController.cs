using ApplicationLayer.Features.Bookings.Commands.AssignWarehouse;
using ApplicationLayer.Features.Bookings.Commands.AssignEmployee;
using ApplicationLayer.Features.Bookings.Commands.CreateBooking;
using ApplicationLayer.Features.Bookings.Dtos;
using ApplicationLayer.Features.Bookings.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch;
using ApplicationLayer.Features.Bookings.Queries.GetBranchBookingsForManager;
using ApplicationLayer.Features.Bookings.Queries.GetMyBookings;
using ApplicationLayer.Features.Bookings.Queries.GetInspectionReportByBooking;
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
    /// <summary>
    /// UC-15: Assign an employee to a booking (shop manager only)
    /// </summary>
    [HttpPost("{bookingId:guid}/assign-employee/{employeeId:guid}")]
    [Authorize]
    public async Task<IActionResult> AssignEmployee(Guid bookingId, Guid employeeId, CancellationToken ct)
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

        var branchIdClaim = User.FindFirstValue("BranchId");
        Guid? actorBranchId = null;
        if (!string.IsNullOrWhiteSpace(branchIdClaim) && Guid.TryParse(branchIdClaim, out var parsedBranchId))
            actorBranchId = parsedBranchId;

        var cmd = new AssignEmployeeCommand(
            ActorUserId: actorId,
            ActorRole: actorRole,
            ActorBranchId: actorBranchId,
            BookingId: bookingId,
            EmployeeId: employeeId
        );

        var result = await _mediator.Send(cmd, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Get inspection report for a booking. Returns a response object with nullable report instead of throwing when missing.
    /// </summary>
    [HttpGet("{bookingId:guid}/inspection-report")]
    [Authorize(Roles = "ShopManager,ShopOwner")]
    public async Task<IActionResult> GetInspectionReport(Guid bookingId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInspectionReportByBookingQuery(bookingId), ct);

        if (result == null)
            return NotFound();

        if (!result.IsSuccess)
        {
            var msg = result.ErrorMessage ?? string.Empty;

            if (msg.Contains("authenticated", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { error = msg });

            if (msg.Contains("access", StringComparison.OrdinalIgnoreCase) || msg.Contains("admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return BadRequest(new { error = msg });
        }

        return Ok(result.Data);
    }

}

