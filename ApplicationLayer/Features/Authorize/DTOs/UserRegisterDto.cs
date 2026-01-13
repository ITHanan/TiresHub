using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Authorize.DTOs
{
    public class UserRegisterDto
    {
        public string Name { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string Password { get; set; } = default!;

        public string phone { get; set; } = default!;   
        public UserRole Role { get; set; } // VehicleOwner | ShopOwner
    }
}
