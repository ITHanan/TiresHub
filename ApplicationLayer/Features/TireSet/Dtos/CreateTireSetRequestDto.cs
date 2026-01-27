using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TireSet.Dtos
{
    public class CreateTireSetRequestDto
    {
        public Guid VehicleId { get; set; }
        public TireType TireType { get; set; } // 0 Summer, 1 Winter
        public string Size { get; set; } = default!;
        public string Brand { get; set; } = default!;
        public string? Notes { get; set; }
    }
}
