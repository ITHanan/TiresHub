using ApplicationLayer.Features.Employees.Commands;
using ApplicationLayer.Features.Employees.Validators;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentValidation.TestHelper;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Employees;

public class DeactivateReactivateEmployeeTests
{
    #region Deactivate Validator Tests

    [Fact]
    public void DeactivateValidator_Should_Fail_When_EmployeeId_Is_Empty()
    {
        // Arrange
        var validator = new DeactivateEmployeeCommandValidator();
        var command = new DeactivateEmployeeCommand(Guid.Empty);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("Employee ID is required.");
    }

    [Fact]
    public void DeactivateValidator_Should_Pass_When_EmployeeId_Is_Valid()
    {
        // Arrange
        var validator = new DeactivateEmployeeCommandValidator();
        var command = new DeactivateEmployeeCommand(Guid.NewGuid());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Reactivate Validator Tests

    [Fact]
    public void ReactivateValidator_Should_Fail_When_EmployeeId_Is_Empty()
    {
        // Arrange
        var validator = new ReactivateEmployeeCommandValidator();
        var command = new ReactivateEmployeeCommand(Guid.Empty);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("Employee ID is required.");
    }

    [Fact]
    public void ReactivateValidator_Should_Pass_When_EmployeeId_Is_Valid()
    {
        // Arrange
        var validator = new ReactivateEmployeeCommandValidator();
        var command = new ReactivateEmployeeCommand(Guid.NewGuid());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Deactivate Handler Tests
    [Fact]
    public async Task DeactivateEmployee_Should_Succeed_When_Manager_Owns_Branch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        var manager = ShopOwnerSeed.ShopManager(db, branch);

        var employee = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee.AssignBranch(branch.Id);
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch.Id
        };

        var handler = new DeactivateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new DeactivateEmployeeCommand(employee.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsActive);

        // Verify in database
        var updatedEmployee = await db.Users.FindAsync(employee.Id);
        Assert.NotNull(updatedEmployee);
        Assert.False(updatedEmployee.IsActive);
    }

    [Fact]
    public async Task DeactivateEmployee_Should_Fail_When_Not_Authenticated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var employeeId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = false };

        var handler = new DeactivateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new DeactivateEmployeeCommand(employeeId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task DeactivateEmployee_Should_Fail_When_Not_ShopManager()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);

        var employee = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee.AssignBranch(branch.Id);
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = ownerId,
            Role = UserRole.ShopOwner,
            BranchId = branch.Id
        };

        var handler = new DeactivateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new DeactivateEmployeeCommand(employee.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Only shop managers", ex.Message);
    }

    [Fact]
    public async Task DeactivateEmployee_Should_Fail_When_Employee_Not_Found()
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

        var handler = new DeactivateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new DeactivateEmployeeCommand(Guid.NewGuid());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Employee not found", ex.Message);
    }

    [Fact]
    public async Task DeactivateEmployee_Should_Fail_When_User_Is_Not_Employee()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        var manager = ShopOwnerSeed.ShopManager(db, branch);

        var vehicleOwner = new User("Owner", "owner@test.com", null, UserRole.VehicleOwner);
        db.Users.Add(vehicleOwner);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch.Id
        };

        var handler = new DeactivateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new DeactivateEmployeeCommand(vehicleOwner.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("not an employee", ex.Message);
    }

    [Fact]
    public async Task DeactivateEmployee_Should_Fail_When_Employee_In_Different_Branch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch1 = ShopOwnerSeed.Branch(db, company, "Branch1");
        var branch2 = ShopOwnerSeed.Branch(db, company, "Branch2");

        var manager = ShopOwnerSeed.ShopManager(db, branch1);

        var employee = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee.AssignBranch(branch2.Id);
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch1.Id
        };

        var handler = new DeactivateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new DeactivateEmployeeCommand(employee.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("only manage employees in your own branch", ex.Message);
    }

    #endregion

    #region Reactivate Handler Tests

    [Fact]
    public async Task ReactivateEmployee_Should_Succeed_When_Manager_Owns_Branch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId);
        var branch = ShopOwnerSeed.Branch(db, company);
        var manager = ShopOwnerSeed.ShopManager(db, branch);

        var employee = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee.AssignBranch(branch.Id);
        employee.Deactivate();
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager,
            BranchId = branch.Id
        };

        var handler = new ReactivateEmployeeCommandHandler(
            currentUser,
            new UserRepository(db),
            new BranchRepository(db),
            new AuditRepository(db)
        );

        var command = new ReactivateEmployeeCommand(employee.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);

        // Verify in database
        var updatedEmployee = await db.Users.FindAsync(employee.Id);
        Assert.NotNull(updatedEmployee);
        Assert.True(updatedEmployee.IsActive);
    }

    #endregion
}
