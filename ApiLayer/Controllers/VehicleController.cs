using ApplicationLayer.Features.Vehicle.Command;
using ApplicationLayer.Features.Vehicle.Dtos;
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

       // [Authorize(Roles = "VehicleOwner")]
        [HttpGet]
        public async Task<IActionResult> GetMyVehicles()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdString))
                return Unauthorized("Invalid token");

            var ownerId = Guid.Parse(userIdString);

            var result = await _mediator.Send(
                new GetMyVehiclesQuery(ownerId));

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result.Data);
        }
    }
}
