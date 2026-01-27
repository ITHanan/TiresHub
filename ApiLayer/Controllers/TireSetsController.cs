using ApplicationLayer.Features.TireSet.Command;
using ApplicationLayer.Features.TireSet.Command.CeateTire;
using ApplicationLayer.Features.TireSet.Command.UpdateTireSet;
using ApplicationLayer.Features.TireSet.Dtos;
using ApplicationLayer.Features.TireSet.Queries;
using DomainLayer.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiLayer.Controllers
{
    [Route("api/tiresets")]
    [ApiController]
    [Authorize]
    public class TireSetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TireSetsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTireSetRequestDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized("Invalid token.");

            var ownerId = Guid.Parse(userIdString);

            var command = new CreateTireSetCommand(
                OwnerId: ownerId,
                VehicleId: dto.VehicleId,
                TireType: dto.TireType,
                Size: dto.Size,
                Brand: dto.Brand,
                Notes: dto.Notes
            );

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet("by-vehicle/{vehicleId:guid}")]
        public async Task<IActionResult> GetByVehicle(Guid vehicleId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized("Invalid token.");

            var ownerId = Guid.Parse(userIdString);

            var result = await _mediator.Send(new GetVehicleTireSetsQuery(ownerId, vehicleId));

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result.Data);
        }

        [HttpPut("{tireSetId:guid}")]
        public async Task<IActionResult> Update(
               Guid tireSetId,
               [FromBody] UpdateTireSetRequest request,
               CancellationToken ct)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var role = Enum.Parse<UserRole>(
                User.FindFirstValue(ClaimTypes.Role)!);

            var command = new UpdateTireSetCommand(
                ActorUserId: userId,
                ActorRole: role,
                TireSetId: tireSetId,
                Size: request.Size,
                Brand: request.Brand,
                Notes: request.Notes
            );

            var result = await _mediator.Send(command, ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.ErrorMessage });

            return NoContent();
        }
    }
}
