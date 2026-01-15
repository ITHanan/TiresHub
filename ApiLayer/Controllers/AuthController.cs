using ApplicationLayer.Features.Authorize.Commands.Register;
using ApplicationLayer.Features.Authorize.DTOs;
using ApplicationLayer.Features.Authorize.Queries.Login;
using ApplicationLayer.Features.Onboarding.Commands;
using ApplicationLayer.Features.StartAuth.Commands;
using ApplicationLayer.Features.StartAuth.Commands.VerifyCode;
using ApplicationLayer.Features.StartAuth.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            var command = new RegisterCommand(
                userRegisterDto.Name,
                userRegisterDto.UserEmail,
                userRegisterDto.Password,
                userRegisterDto.phone,
                userRegisterDto.Role
            );
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var query = new LoginQuery(userLoginDto.UserEmail, userLoginDto.Password);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("start")]
        public async Task<IActionResult> StartAuth([FromBody] StartAuthRequestDto dto)
        {
            var command = new StartAuthCommand(
                dto.Identifier,
                dto.Role
            );

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequestDto dto)
        {
            var command = new VerifyCodeCommand(
                dto.Identifier,
                dto.Code
            );

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("onboarding/complete")]
        public async Task<IActionResult> CompleteOnboarding()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _mediator.Send(
                new CompleteOnboardingCommand(userId));

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok();
        }

        /// <summary>
        /// Logs out the currently authenticated user.
        /// JWT is stateless, so logout is handled client-side.
        /// This endpoint exists for consistency, auditing, and future extensions.
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Optional (recommended later):
            // _auditService.LogLogout(userId);

            return Ok(new
            {
                message = "Logged out successfully"
            });
        }
    }
}
