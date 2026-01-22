using ApplicationLayer.Managers.Commands.RegisterShopManager;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/ShopManagers")]
    [Authorize]
    public class ManagersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManagersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterShopManagerCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
