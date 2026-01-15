using ApplicationLayer.Features.StartAuth.Dtos;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StartAuth.Commands.VerifyCode
{
    public record VerifyCodeCommand(string Identifier, string Code)
     : IRequest<OperationResult<AuthResponseDto>>;
}
