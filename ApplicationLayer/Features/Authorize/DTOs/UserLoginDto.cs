using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Authorize.DTOs
{
    public class UserLoginDto
    {
        public required string UserEmail { get; set; }
        public required string Password { get; set; }
    }
}
