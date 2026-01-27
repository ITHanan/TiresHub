using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TireSet.Dtos
{
    public record TireSetDto(
      Guid Id,
      Guid VehicleId,
      string TireType,
      string Size,
      string Brand,
      string? Notes,
      bool IsLocked,
      DateTime CreatedAt
  );
}
