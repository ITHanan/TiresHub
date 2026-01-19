using ApplicationLayer.Common;
using ApplicationLayer.Common.Mappings;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiLayer.Mappings;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var idString =
                user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user?.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idString))
                throw new InvalidOperationException("No authenticated user id found in claims.");

            return Guid.Parse(idString);
        }
    }
}
