

using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;

namespace Tests.ShopownerTests.CompanyTests;

public class RegisterShopTests
{
    [Fact]
    public async Task RegisterCompany_Should_CreateCompany_ForCurrentUser()
    {
        using var db = TestDbFactory.CreateDb();
        var currentUserId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = currentUserId };
        var handler = new RegisterCompanyCommandHandler(
            currentUser,
            new UserRepository(db),
            new CompanyRepository(db)
        );
      
    }

}