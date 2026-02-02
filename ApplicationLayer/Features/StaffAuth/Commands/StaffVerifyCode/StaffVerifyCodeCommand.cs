using ApplicationLayer.Features.StaffAuth.Dtos;
using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StaffAuth.Commands.StaffVerifyCode
{
    public record StaffVerifyCodeCommand(
       string Identifier,
       string Code,
       UserRole Role
   ) : IRequest<OperationResult<StaffAuthResponseDto>>;
}
