using ApplicationLayer.Managers.Commands.RegisterShopManager;
using DomainLayer.Enums;
using DomainLayer.Users;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.ManagerTests;

public class RegisterShopManagerCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser { IsAuthenticated = false, UserId = ownerId };

        var handler = new RegisterShopManagerCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db),
            new UserRepository(db)
        );

        var cmd = new RegisterShopManagerCommand(
            Name: "Manager Name",
            Email: "manager@test.se",
            Phone: null,
            BranchId: new List<Guid> { branch.Id }
        );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNameMissing_ShouldThrowInvalidOperation()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var handler = new RegisterShopManagerCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db),
            new UserRepository(db)
        );

        var cmd = new RegisterShopManagerCommand(
            Name: "   ",
            Email: "manager@test.se",
            Phone: null,
            BranchId: new List<Guid> { branch.Id }
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(cmd, CancellationToken.None));

        Assert.Contains("Name is required", ex.Message);
    }

    [Fact]
    public async Task Handle_WhenBranchListNullOrEmpty_ShouldThrowInvalidOperation()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var handler = new RegisterShopManagerCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db),
            new UserRepository(db)
        );

        var cmdNull = new RegisterShopManagerCommand(
            Name: "Manager",
            Email: "manager@test.se",
            Phone: null,
            BranchId: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(cmdNull, CancellationToken.None));

        var cmdEmpty = new RegisterShopManagerCommand(
            Name: "Manager",
            Email: "manager@test.se",
            Phone: null,
            BranchId: new List<Guid>()
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(cmdEmpty, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNoEmailAndNoPhone_ShouldThrowInvalidOperation()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var handler = new RegisterShopManagerCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db),
            new UserRepository(db)
        );

        var cmd = new RegisterShopManagerCommand(
            Name: "Manager",
            Email: null,
            Phone: null,
            BranchId: new List<Guid> { branch.Id }
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(cmd, CancellationToken.None));

        Assert.Contains("Email or phone is required", ex.Message);
    }

    [Fact]
    public async Task Handle_WhenPhoneOnly_ShouldThrowInvalidOperation()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var handler = new RegisterShopManagerCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db),
            new UserRepository(db)
        );

        var cmd = new RegisterShopManagerCommand(
            Name: "Manager",
            Email: null,
            Phone: "0700000000",
            BranchId: new List<Guid> { branch.Id }
        );
    }
}