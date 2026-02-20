using ApplicationLayer.Features.Bookings.Queries.GetAssignedBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/employees/me")]
public sealed class EmployeeTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeeTasksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get bookings assigned to the current employee (sorted ascending by appointment date).

    [HttpGet("assigned-bookings")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> GetAssignedBookings(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAssignedBookingsQuery(), ct);
        return Ok(result);
    }
}