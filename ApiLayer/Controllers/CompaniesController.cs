using ApplicationLayer.Features.Companies.Commands;
using ApplicationLayer.Features.Companies.Queries.GetCompanyById;
using ApplicationLayer.Features.Companies.Queries.GetMyCompanies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCompanyCommand command,
        CancellationToken ct)
    {
        var companyId = await _mediator.Send(command, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = companyId },
            new { id = companyId }
        );
    }
    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken ct)
    {
        var companies = await _mediator.Send(new GetMyCompaniesQuery(), ct);
        return Ok(companies);

    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var company = await _mediator.Send(
            new GetCompanyByIdQuery(id),
            ct
        );

        return company is null ? NotFound() : Ok(company);
    }
}
