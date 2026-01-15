using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StartAuth.Dtos
{
    public record StartAuthRequestDto(
        string Identifier,
        UserRole Role
    );
}
