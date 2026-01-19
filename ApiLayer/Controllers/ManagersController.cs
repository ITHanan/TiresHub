using ApplicationLayer.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/managers")]
[Authorize(Roles = "ShopOwner")]
public class ManagersController : ControllerBase
{
    private readonly IShopManagerService _service;
    public ManagersController(IShopManagerService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateShopManagerRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpPut("{managerId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid managerId, DeactivateShopManagerRequest request, CancellationToken ct)
    {
        await _service.SetActiveAsync(managerId, request.IsActive, ct);
        return NoContent();
    }
}
