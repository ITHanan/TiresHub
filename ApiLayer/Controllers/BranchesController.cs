using ApplicationLayer.Features.Branches.Commands.CreateBranch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/branches")]
    [Authorize]
    public class BranchesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BranchesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBranchCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
