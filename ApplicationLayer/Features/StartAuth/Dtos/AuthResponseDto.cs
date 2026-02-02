using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StartAuth.Dtos
{
    public record AuthResponseDto(
    string Token,
    UserDto User
)
    {
        public bool IsFirstLogin { get; set; }
    }

    public record UserDto(
        Guid Id,
        string? Email,
        string? Phone,
        int Role,
        bool IsFirstLogin,
        bool OnboardingCompleted
    );

}
