using ApplicationLayer.Features.Authorize.Commands.Register;
using ApplicationLayer.Features.Authorize.DTOs;
using ApplicationLayer.Features.Authorize.Queries.Login;
using ApplicationLayer.Features.StartAuth.Commands;
using ApplicationLayer.Features.StartAuth.Commands.VerifyCode;
using ApplicationLayer.Features.StartAuth.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
