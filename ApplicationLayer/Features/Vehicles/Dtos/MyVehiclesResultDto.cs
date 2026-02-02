using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Dtos
{
    public record MyVehiclesResultDto
   (
       List<VehicleDto> ActiveVehicles,
       List<VehicleDto> InactiveVehicles
   );
}
