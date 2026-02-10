using ApplicationLayer.Features.Vehicle.Dtos;
using ApplicationLayer.Features.Vehicles.Command.ActivateVehicle;
using ApplicationLayer.Features.Vehicles.Command.CreateVehicle;
using ApplicationLayer.Features.Vehicles.Command.DeactivateVehicle;
using ApplicationLayer.Features.Vehicles.Command.UpdateVehicle;
using ApplicationLayer.Features.Vehicles.Dtos;
using ApplicationLayer.Features.Vehicles.Queries;
using DomainLayer.Vehicles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "VehicleOwner")]
    public class VehicleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VehicleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(CreateVehicleRequestDtos dto)
        {
            // Implementation for creating a vehicle goes here

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized("Invalid token");

            var ownerId = Guid.Parse(userIdString);

            var command = new CreateVehicleCommand(
                ownerId,
                dto.PlateNumber,
                dto.Make,
                dto.Model,
                dto.Year
            );

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetMyVehicles()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized("Invalid token");

            if (!Guid.TryParse(userIdString, out var ownerId))
                return Unauthorized("Invalid user identifier");

            var result = await _mediator.Send(new GetMyVehiclesQuery(ownerId));

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result.Data); // MyVehiclesResultDto
        }


        // PUT: /api/vehicle/{vehicleId}
        [HttpPut("{vehicleId:guid}")]
        public async Task<IActionResult> UpdateVehicle(
            Guid vehicleId,
            [FromBody] UpdateVehicleRequestDto dto)
        {
            var ownerId = GetUserId();

            var command = new UpdateVehicleCommand(
                OwnerId: ownerId,
                VehicleId: vehicleId,
                Make: dto.Make,
                Model: dto.Model,
                Year: dto.Year
            );

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    error = result.ErrorMessage
                });

            return Ok(new
            {
                success = true,
                message = "Vehicle updated successfully."
            });
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            return Guid.Parse(userId);
        }


        // PATCH /api/Vehicle/{vehicleId}/activate
        [HttpPatch("{vehicleId:guid}/activate")]
        public async Task<IActionResult> ActivateVehicle(Guid vehicleId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized("Invalid token");

            var ownerId = Guid.Parse(userIdString);

            var result = await _mediator.Send(
                new ActivateVehicleCommand(ownerId, vehicleId));

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized("Invalid token");

            if (!Guid.TryParse(userIdString, out var ownerId))
                return Unauthorized("Invalid token");

            var result = await _mediator.Send(
                new DeactivateVehicleCommand(ownerId, id),
                ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
