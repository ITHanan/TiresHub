using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StaffAuth.Dtos
{
    public record StaffVerifyCodeDto(string Identifier, string Code, UserRole Role);
   
}
