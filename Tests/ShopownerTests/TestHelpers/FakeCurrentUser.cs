using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
namespace Tests.ShopownerTests.TestHelpers;

public class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public Guid UserId { get; set; } = Guid.NewGuid();
    public UserRole Role { get; set; } = UserRole.ShopOwner; // ändra om du har annan enum
}
