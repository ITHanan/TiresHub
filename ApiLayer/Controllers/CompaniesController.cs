using ApplicationLayer.Features.Companies.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
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
            return CreatedAtAction(nameof(GetById), new { id = companyId }, null);
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id) => Ok();
    }
}
