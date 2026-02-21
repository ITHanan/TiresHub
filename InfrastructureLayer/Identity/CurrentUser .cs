using System.Security.Claims;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace InfrastructureLayer.Identity
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUser(IHttpContextAccessor http)
        {
            _http = http;
        }

        public bool IsAuthenticated =>
            _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public Guid UserId
        {
            get
            {
                var value = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(value, out var id) ? id : Guid.Empty;
            }
        }

        public UserRole Role
        {
            get
            {
                var value = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrWhiteSpace(value))
                    throw new UnauthorizedAccessException("User role claim is missing.");

                if (!Enum.TryParse<UserRole>(value, ignoreCase: true, out var role))
                    throw new UnauthorizedAccessException("Invalid user role claim.");

                return role;
            }
        }


        public Guid? BranchId
        {
            get
            {
                var value = _http.HttpContext?.User?.FindFirstValue("BranchId");
                if (string.IsNullOrWhiteSpace(value))
                    return null;
                return Guid.TryParse(value, out var branchId) ? branchId : null;
            }
        }
    }
}
