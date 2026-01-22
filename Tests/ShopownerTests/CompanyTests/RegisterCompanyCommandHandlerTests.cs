using ApplicationLayer.Features.Companies.Commands;
using DomainLayer.Users;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.CompanyTests;

public class RegisterCompanyCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrow()
    {
        using var db = TestDbFactory.CreateDb();

        // current user finns INTE i Users-tabellen
        var currentUserId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = currentUserId
        };

        var handler = new RegisterCompanyCommandHandler(
            currentUser,
            new UserRepository(db),
            new CompanyRepository(db)
        );

        var cmd = new RegisterCompanyCommand("Test Company");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(cmd, CancellationToken.None));

        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldCreateCompany_AndReturnId()
    {
        using var db = TestDbFactory.CreateDb();

        // ✅ Seed user (krävs av handlern)
        var userId = Guid.NewGuid();
        var user = new User(
            name: "Owner",
            email: "owner@test.se",
            phone: null,
            role: DomainLayer.Enums.UserRole.ShopOwner
        );

        // Viktigt: sätt Id manuellt om din User tillåter det
        typeof(User)
            .GetProperty("Id")!
            .SetValue(user, userId);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = userId
        };

        var handler = new RegisterCompanyCommandHandler(
            currentUser,
            new UserRepository(db),
            new CompanyRepository(db)
        );

        var cmd = new RegisterCompanyCommand("My First Company");

        // ACT
        var companyId = await handler.Handle(cmd, CancellationToken.None);

        // ASSERT
        Assert.NotEqual(Guid.Empty, companyId);

        var company = db.ShopCompanies.Single(c => c.Id == companyId);
        Assert.Equal("My First Company", company.Name);
        Assert.Equal(userId, company.OwnerId);
    }
}

