using ApplicationLayer.Features.Employees.Commands;
using ApplicationLayer.Features.Employees.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ShopManager")]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new employee account for the manager's branch
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(
            [FromBody] CreateEmployeeCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get all employees for the manager's branch
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBranchEmployees(CancellationToken ct)
        {
            var query = new GetBranchEmployeesQuery();
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Deactivate an employee account
        /// </summary>
        [HttpPost("{employeeId}/deactivate")]
        public async Task<IActionResult> DeactivateEmployee(
            Guid employeeId,
            CancellationToken ct)
        {
            var command = new DeactivateEmployeeCommand(employeeId);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Reactivate an employee account
        /// </summary>
        [HttpPost("{employeeId}/reactivate")]
        public async Task<IActionResult> ReactivateEmployee(
            Guid employeeId,
            CancellationToken ct)
        {
            var command = new ReactivateEmployeeCommand(employeeId);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
