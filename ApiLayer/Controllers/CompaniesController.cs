using ApplicationLayer.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize(Roles = "ShopOwner")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;
    public CompaniesController(ICompanyService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<Guid>> Register(RegisterCompanyRequest request, CancellationToken ct)
        => Ok(await _service.RegisterCompanyAsync(request, ct));
}
