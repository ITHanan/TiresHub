using ApplicationLayer.Features.Employees.Commands;
using ApplicationLayer.Features.Employees.Queries;
using DomainLayer.Enums;
using DomainLayer.Users;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Employees;

public class CreateEmployeeTests
{
    [Fact]
    public async Task CreateEmployee_Should_Succeed_When_Manager_Authenticated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        var manager = ShopOwnerSeed.ShopManager(db, branch, "Manager", "mgr@test.com");
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch.Id
        };

        var handler = new CreateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new CreateEmployeeCommand(
            Name: "John Employee",
            Email: "john@test.com",
            Phone: "0700000000"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Employee", result.Name);
        Assert.Equal("john@test.com", result.Email);
        Assert.Equal(branch.Id, result.BranchId);
        Assert.True(result.IsActive);
        Assert.Equal(UserRole.Employee, result.Role);

        // Verify in database
        var employee = await db.Users.FindAsync(result.Id);
        Assert.NotNull(employee);
        Assert.Equal(UserRole.Employee, employee.Role);
        Assert.Equal(branch.Id, employee.BranchId);
    }

    [Fact]
    public async Task CreateEmployee_Should_Fail_When_Not_Authenticated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = false
        };

        var handler = new CreateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new CreateEmployeeCommand(
            Name: "John Employee",
            Email: "john@test.com",
            Phone: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateEmployee_Should_Fail_When_Not_ShopManager()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = ownerId,
            Role = UserRole.ShopOwner
        };

        var handler = new CreateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new CreateEmployeeCommand(
            Name: "John Employee",
            Email: "john@test.com",
            Phone: null
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Only shop managers", ex.Message);
    }

    [Fact]
    public async Task CreateEmployee_Should_Fail_When_Name_Missing()
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

        var handler = new CreateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new CreateEmployeeCommand(
            Name: "   ",
            Email: "john@test.com",
            Phone: null
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Name is required", ex.Message);
    }

    [Fact]
    public async Task CreateEmployee_Should_Fail_When_No_Email_And_No_Phone()
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

        var handler = new CreateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new CreateEmployeeCommand(
            Name: "John Employee",
            Email: null,
            Phone: null
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Email or phone is required", ex.Message);
    }

    [Fact]
    public async Task CreateEmployee_Should_Fail_When_Manager_Has_No_Branch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var managerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager,
            BranchId = null
        };

        var handler = new CreateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new CreateEmployeeCommand(
            Name: "John Employee",
            Email: "john@test.com",
            Phone: null
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Shop manager must be assigned to a branch", ex.Message);
    }

    [Fact]
    public async Task CreateEmployee_Should_Prevent_Cross_Branch_Assignment()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch1 = ShopOwnerSeed.Branch(db, company, "Branch1");
        var branch2 = ShopOwnerSeed.Branch(db, company, "Branch2");
        
        // Create employee already assigned to branch1
        var existingEmployee = new User("Jane Employee", "jane@test.com", null, UserRole.Employee);
        existingEmployee.AssignBranch(branch1.Id);
        db.Users.Add(existingEmployee);

        // Create manager for branch2
        var manager = ShopOwnerSeed.ShopManager(db, branch2);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch2.Id
        };

        var handler = new CreateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new CreateEmployeeCommand(
            Name: "Jane Employee",
            Email: "jane@test.com",
            Phone: null
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("already assigned to another branch", ex.Message);
    }
}
