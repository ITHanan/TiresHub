using ApplicationLayer.Features.Bookings.Commands.AssignWarehouse;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.shops;
using DomainLayer.Users;
using DomainLayer.Vehicles;
using FluentAssertions;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.BookingTests;

public class AssignWarehouseToBookingTests
{
    [Fact]
    public async Task AssignWarehouse_Should_Succeed_WhenManagerOwnsBranchAndWarehouseIsAvailable()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager
        };

        // Create company and branch
        var company = new ShopCompany("TestCo", ownerId);
        db.ShopCompanies.Add(company);

        var branch = new Branch("Branch1", "City", "Address", company.Id);
        db.Branches.Add(branch);

        // Create manager assigned to the branch
        var manager = new User("Manager", "manager@test.com", null, UserRole.ShopManager);
        manager.AssignBranch(branch.Id);
        db.Users.Add(manager);

        // Create warehouse with available capacity
        var warehouse = new Warehouse("Warehouse A", 10, branch.Id);
        db.Warehouses.Add(warehouse);

        // Create vehicle owner
        var vehicleOwner = new User("Owner", "owner@test.com", null, UserRole.VehicleOwner);
        db.Users.Add(vehicleOwner);

        // Create vehicle
        var vehicle = new Vehicle("ABC123", vehicleOwnerId);
        db.Vehicles.Add(vehicle);

        // Create booking
        var booking = new Booking(ServiceType.ChangeTires, DateTime.Now.AddDays(1), vehicle.Id, branch.Id);
        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        // Update current user with correct manager ID
        currentUser.UserId = manager.Id;

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(booking.Id, warehouse.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(booking.Id);
        result.WarehouseId.Should().Be(warehouse.Id);

        // Verify booking was updated
        var updatedBooking = await db.Bookings.FindAsync(booking.Id);
        updatedBooking.Should().NotBeNull();
        updatedBooking!.WarehouseId.Should().Be(warehouse.Id);

        // Verify warehouse usage was increased
        var updatedWarehouse = await db.Warehouses.FindAsync(warehouse.Id);
        updatedWarehouse.Should().NotBeNull();
        updatedWarehouse!.CurrentUsage.Should().Be(1);
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = false
        };

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Not authenticated");
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowUnauthorized_WhenUserIsNotShopManager()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            Role = UserRole.Employee // Not a ShopManager
        };

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Only shop managers can assign storage locations");
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowInvalidOperation_WhenBookingNotFound()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            Role = UserRole.ShopManager
        };

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Booking not found");
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowInvalidOperation_WhenBookingIsNotConfirmed()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var managerId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager
        };

        // Create company and branch
        var ownerId = Guid.NewGuid();
        var company = new ShopCompany("TestCo", ownerId);
        db.ShopCompanies.Add(company);

        var branch = new Branch("Branch1", "City", "Address", company.Id);
        db.Branches.Add(branch);

        // Create vehicle
        var vehicle = new Vehicle("ABC123", vehicleOwnerId);
        db.Vehicles.Add(vehicle);

        // Create booking and cancel it
        var booking = new Booking(ServiceType.ChangeTires, DateTime.Now.AddDays(1), vehicle.Id, branch.Id);
        booking.Cancel(); // Change status from Confirmed
        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(booking.Id, Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Only confirmed bookings can be assigned to warehouse");
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowInvalidOperation_WhenWarehouseNotFound()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var managerId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager
        };

        // Create company and branch
        var ownerId = Guid.NewGuid();
        var company = new ShopCompany("TestCo", ownerId);
        db.ShopCompanies.Add(company);

        var branch = new Branch("Branch1", "City", "Address", company.Id);
        db.Branches.Add(branch);

        // Create vehicle
        var vehicle = new Vehicle("ABC123", vehicleOwnerId);
        db.Vehicles.Add(vehicle);

        // Create booking
        var booking = new Booking(ServiceType.ChangeTires, DateTime.Now.AddDays(1), vehicle.Id, branch.Id);
        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(booking.Id, Guid.NewGuid()); // Non-existent warehouse

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Warehouse not found");
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowInvalidOperation_WhenWarehouseIsFull()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var managerId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager
        };

        // Create company and branch
        var ownerId = Guid.NewGuid();
        var company = new ShopCompany("TestCo", ownerId);
        db.ShopCompanies.Add(company);

        var branch = new Branch("Branch1", "City", "Address", company.Id);
        db.Branches.Add(branch);

        // Create manager assigned to the branch
        var manager = new User("Manager", "manager@test.com", null, UserRole.ShopManager);
        manager.AssignBranch(branch.Id);
        db.Users.Add(manager);

        // Create warehouse with capacity = 1 and fill it
        var warehouse = new Warehouse("Warehouse A", 1, branch.Id);
        warehouse.IncreaseUsage(); // Make it full
        db.Warehouses.Add(warehouse);

        // Create vehicle
        var vehicle = new Vehicle("ABC123", vehicleOwnerId);
        db.Vehicles.Add(vehicle);

        // Create booking
        var booking = new Booking(ServiceType.ChangeTires, DateTime.Now.AddDays(1), vehicle.Id, branch.Id);
        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        // Update current user with correct manager ID
        currentUser.UserId = manager.Id;

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(booking.Id, warehouse.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Warehouse is full");
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowUnauthorized_WhenWarehouseBelongsToDifferentBranch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var managerId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager
        };

        // Create company and branches
        var ownerId = Guid.NewGuid();
        var company = new ShopCompany("TestCo", ownerId);
        db.ShopCompanies.Add(company);

        var branch1 = new Branch("Branch1", "City1", "Address1", company.Id);
        var branch2 = new Branch("Branch2", "City2", "Address2", company.Id);
        db.Branches.Add(branch1);
        db.Branches.Add(branch2);

        // Create manager assigned to branch1
        var manager = new User("Manager", "manager@test.com", null, UserRole.ShopManager);
        manager.AssignBranch(branch1.Id);
        db.Users.Add(manager);

        // Create warehouse in branch2
        var warehouse = new Warehouse("Warehouse A", 10, branch2.Id);
        db.Warehouses.Add(warehouse);

        // Create vehicle
        var vehicle = new Vehicle("ABC123", vehicleOwnerId);
        db.Vehicles.Add(vehicle);

        // Create booking in branch1
        var booking = new Booking(ServiceType.ChangeTires, DateTime.Now.AddDays(1), vehicle.Id, branch1.Id);
        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        // Update current user with correct manager ID
        currentUser.UserId = manager.Id;

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(booking.Id, warehouse.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Cannot assign warehouse from a different branch");
    }

    [Fact]
    public async Task AssignWarehouse_Should_ThrowUnauthorized_WhenManagerNotAssignedToBookingBranch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var managerId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager
        };

        // Create company and branches
        var ownerId = Guid.NewGuid();
        var company = new ShopCompany("TestCo", ownerId);
        db.ShopCompanies.Add(company);

        var branch1 = new Branch("Branch1", "City1", "Address1", company.Id);
        var branch2 = new Branch("Branch2", "City2", "Address2", company.Id);
        db.Branches.Add(branch1);
        db.Branches.Add(branch2);

        // Create manager assigned to branch2
        var manager = new User("Manager", "manager@test.com", null, UserRole.ShopManager);
        manager.AssignBranch(branch2.Id);
        db.Users.Add(manager);

        // Create warehouse in branch1
        var warehouse = new Warehouse("Warehouse A", 10, branch1.Id);
        db.Warehouses.Add(warehouse);

        // Create vehicle
        var vehicle = new Vehicle("ABC123", vehicleOwnerId);
        db.Vehicles.Add(vehicle);

        // Create booking in branch1
        var booking = new Booking(ServiceType.ChangeTires, DateTime.Now.AddDays(1), vehicle.Id, branch1.Id);
        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        // Update current user with correct manager ID
        currentUser.UserId = manager.Id;

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(booking.Id, warehouse.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("You can only assign warehouses for bookings in your assigned branch");
    }

    [Fact]
    public async Task AssignWarehouse_Should_IncrementWarehouseUsage()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var managerId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = managerId,
            Role = UserRole.ShopManager
        };

        // Create company and branch
        var ownerId = Guid.NewGuid();
        var company = new ShopCompany("TestCo", ownerId);
        db.ShopCompanies.Add(company);

        var branch = new Branch("Branch1", "City", "Address", company.Id);
        db.Branches.Add(branch);

        // Create manager assigned to the branch
        var manager = new User("Manager", "manager@test.com", null, UserRole.ShopManager);
        manager.AssignBranch(branch.Id);
        db.Users.Add(manager);

        // Create warehouse
        var warehouse = new Warehouse("Warehouse A", 10, branch.Id);
        db.Warehouses.Add(warehouse);

        // Create vehicle
        var vehicle = new Vehicle("ABC123", vehicleOwnerId);
        db.Vehicles.Add(vehicle);

        // Create booking
        var booking = new Booking(ServiceType.ChangeTires, DateTime.Now.AddDays(1), vehicle.Id, branch.Id);
        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        var initialUsage = warehouse.CurrentUsage;

        // Update current user with correct manager ID
        currentUser.UserId = manager.Id;

        var handler = new AssignWarehouseToBookingCommandHandler(
            new BookingRepository(db),
            new WarehouseRepository(db),
            new BranchRepository(db),
            new CompanyRepository(db),
            new UserRepository(db),
            currentUser
        );

        var command = new AssignWarehouseToBookingCommand(booking.Id, warehouse.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedWarehouse = await db.Warehouses.FindAsync(warehouse.Id);
        updatedWarehouse.Should().NotBeNull();
        updatedWarehouse!.CurrentUsage.Should().Be(initialUsage + 1);
    }
}
