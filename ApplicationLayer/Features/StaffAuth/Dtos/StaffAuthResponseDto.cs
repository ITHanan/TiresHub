using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StaffAuth.Dtos
{
    public record StaffAuthResponseDto(string Token, string Role, Guid BranchId);
  
}
