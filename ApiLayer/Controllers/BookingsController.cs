using ApplicationLayer.Features.Bookings.Commands.AssignWarehouse;
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
        /// Assign a warehouse to a booking. Only shop managers can perform this action.
        /// </summary>
        /// <param name="bookingId">The ID of the booking to assign warehouse to</param>
        /// <param name="command">The command containing the warehouse ID to assign</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Updated booking details with assigned warehouse</returns>
        [HttpPost("{bookingId}/assign-warehouse")]
        public async Task<IActionResult> AssignWarehouse(
            Guid bookingId,
            [FromBody] AssignWarehouseRequest request,
            CancellationToken ct)
        {
            var command = new AssignWarehouseToBookingCommand(bookingId, request.WarehouseId);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }

    /// <summary>
    /// Request body for assigning a warehouse to a booking
    /// </summary>
    public record AssignWarehouseRequest(Guid WarehouseId);
}
