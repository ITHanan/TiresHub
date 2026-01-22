using ApplicationLayer.Features.Warehouses.Commands;
using ApplicationLayer.Features.Warehouses.Commands.CreateWarehouse;
using ApplicationLayer.Features.Warehouses.Commands.Usage;
using ApplicationLayer.Warehouses.Queries.GetWarehouses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/warehouses")]
    [Authorize]
    public class WarehousesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehousesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a warehouse under a branch
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWarehouseCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        [HttpPost("{warehouseId}/usage/increase")]
        public async Task<IActionResult> IncreaseUsage(Guid warehouseId, CancellationToken ct)
        {
            await _mediator.Send(new IncreaseWarehouseUsageCommand(warehouseId), ct);
            return NoContent();
        }

        [HttpPost("{warehouseId}/usage/decrease")]
        public async Task<IActionResult> DecreaseUsage(Guid warehouseId, CancellationToken ct)
        {
            await _mediator.Send(new DecreaseWarehouseUsageCommand(warehouseId), ct);
            return NoContent();
        }


        [HttpGet("by-branch/{branchId}")]
         public async Task<IActionResult> GetByBranch(Guid branchId, CancellationToken ct)
         {
        var result = await _mediator.Send(new GetWarehousesByBranchQuery(branchId), ct);
        return Ok(result);
         }

}
}
