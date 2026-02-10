using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Dtos
{
    public record VehicleDto
        (
         Guid Id,
        string PlateNumber,
        string? Make,
        string? Model,
        int? Year,
        DateTime CreatedAt,
        bool IsActive
        );
    
       
    
}
