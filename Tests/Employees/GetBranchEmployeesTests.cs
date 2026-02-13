using ApplicationLayer.Features.Employees.Queries;
using DomainLayer.Enums;
using DomainLayer.Users;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Employees;

public class GetBranchEmployeesTests
{
    [Fact]
    public async Task GetBranchEmployees_Should_Return_All_Employees_For_Branch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        var manager = ShopOwnerSeed.ShopManager(db, branch);

        var employee1 = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee1.AssignBranch(branch.Id);

        var employee2 = new User("Jane Employee", "jane@test.com", "0700000000", UserRole.Employee);
        employee2.AssignBranch(branch.Id);
        employee2.Deactivate();

        db.Users.AddRange(employee1, employee2);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch.Id
        };

        var handler = new GetBranchEmployeesQueryHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db)
        );

        var query = new GetBranchEmployeesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var john = result.First(e => e.Email == "john@test.com");
        Assert.Equal("John Employee", john.Name);
        Assert.True(john.IsActive);

        var jane = result.First(e => e.Email == "jane@test.com");
        Assert.Equal("Jane Employee", jane.Name);
        Assert.False(jane.IsActive);
    }

    [Fact]
    public async Task GetBranchEmployees_Should_Return_Empty_List_When_No_Employees()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        var manager = ShopOwnerSeed.ShopManager(db, branch);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch.Id
        };

        var handler = new GetBranchEmployeesQueryHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db)
        );

        var query = new GetBranchEmployeesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBranchEmployees_Should_Not_Return_Employees_From_Other_Branch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch1 = ShopOwnerSeed.Branch(db, company, "Branch1");
        var branch2 = ShopOwnerSeed.Branch(db, company, "Branch2");

        var manager1 = ShopOwnerSeed.ShopManager(db, branch1, "Manager1", "mgr1@test.com");

        var employee1 = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee1.AssignBranch(branch1.Id);

        var employee2 = new User("Jane Employee", "jane@test.com", null, UserRole.Employee);
        employee2.AssignBranch(branch2.Id);

        db.Users.AddRange(employee1, employee2);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager1.Id,
            Role = UserRole.ShopManager,
            BranchId = branch1.Id
        };

        var handler = new GetBranchEmployeesQueryHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db)
        );

        var query = new GetBranchEmployeesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("john@test.com", result[0].Email);
    }

    [Fact]
    public async Task GetBranchEmployees_Should_Fail_When_Not_Authenticated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var currentUser = new FakeCurrentUser { IsAuthenticated = false };

        var handler = new GetBranchEmployeesQueryHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db)
        );

        var query = new GetBranchEmployeesQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetBranchEmployees_Should_Fail_When_Not_ShopManager()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = ownerId,
            Role = UserRole.ShopOwner
        };

        var handler = new GetBranchEmployeesQueryHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db)
        );

        var query = new GetBranchEmployeesQuery();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
        Assert.Contains("Only shop managers", ex.Message);
    }
}
