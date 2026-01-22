using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicle.Dtos
{
    public class CreateVehicleRequestDtos
    {
        public string PlateNumber { get; set; } = default!;
        public string? Make { get; set; } = default!;
        public string? Model { get; set; } = default!;
        public int? Year { get; set; }
    }
}
