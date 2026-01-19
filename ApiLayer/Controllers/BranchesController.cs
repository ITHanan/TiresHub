using ApplicationLayer.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize(Roles = "ShopOwner")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _service;
    public BranchesController(IBranchService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateBranchRequest request, CancellationToken ct)
        => Ok(await _service.CreateBranchAsync(request, ct));

    [HttpGet("mine")]
    public async Task<ActionResult<List<BranchDto>>> Mine(CancellationToken ct)
        => Ok(await _service.GetMyBranchesAsync(ct));
}
