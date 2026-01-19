using ApplicationLayer.Capacity;
using ApplicationLayer.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize(Roles = "ShopOwner")]
public class WarehousesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromServices] IWarehouseService svc, CreateWarehouseRequest req, CancellationToken ct)
        => Ok(await svc.CreateAsync(req, ct));

    [HttpPut("{warehouseId:guid}")]
    public async Task<IActionResult> Update([FromServices] IWarehouseService svc, Guid warehouseId, UpdateWarehouseRequest req, CancellationToken ct)
    {
        await svc.UpdateAsync(warehouseId, req, ct);
        return NoContent();
    }

    [HttpPut("{warehouseId:guid}/capacity")]
    public async Task<IActionResult> UpdateCapacity([FromServices] ICapacityService svc, Guid warehouseId, UpdateCapacityRequest req, CancellationToken ct)
    {
        await svc.UpdateWarehouseCapacityAsync(warehouseId, req, ct);
        return NoContent();
    }
}
