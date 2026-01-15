using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StartAuth.Dtos
{
    public record VerifyCodeRequestDto(
       string Identifier,
       string Code
   );
}
