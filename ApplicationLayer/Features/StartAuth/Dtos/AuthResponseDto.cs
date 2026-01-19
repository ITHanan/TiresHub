using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StartAuth.Dtos
{
    public class AuthResponseDto
    {

        public string Token { get; init; } = default!;
        public bool IsFirstLogin { get; init; }  

        public string Role { get; init; } = default!;

        public AuthResponseDto(string token, bool isFirstLogin, string role)
        {
            Token = token;
            IsFirstLogin = isFirstLogin;
            Role = role;
        }
    }
}
