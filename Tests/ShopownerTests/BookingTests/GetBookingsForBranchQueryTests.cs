using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch;
using DomainLayer.Enums;
using InfrastructureLayer.Persistence;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.BookingTests;

public class GetBookingsForBranchQueryTests
{
    [Fact]
    public async Task GetBookingsForBranch_Should_Return_BookingsForAssignedBranch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        var branch1 = ShopOwnerSeed.Branch(db, company, "Branch1");
        var branch2 = ShopOwnerSeed.Branch(db, company, "Branch2");

        var manager = ShopOwnerSeed.ShopManager(db, branch1, "Manager1", "manager1@test.com");
        var vehicleOwner = Guid.NewGuid();
        var vehicle1 = ShopOwnerSeed.Vehicle(db, vehicleOwner, "ABC123");
        var vehicle2 = ShopOwnerSeed.Vehicle(db, vehicleOwner, "XYZ789");
        var vehicle3 = ShopOwnerSeed.Vehicle(db, vehicleOwner, "DEF456");

        // Create bookings for branch1
        var booking1 = ShopOwnerSeed.Booking(db, branch1, vehicle1, ServiceType.ChangeTires, DateTime.UtcNow.AddDays(1));
        var booking2 = ShopOwnerSeed.Booking(db, branch1, vehicle2, ServiceType.BuyNewTires, DateTime.UtcNow.AddDays(2));

        // Create booking for branch2 (should not be returned)
        var booking3 = ShopOwnerSeed.Booking(db, branch2, vehicle3, ServiceType.ChangeTires, DateTime.UtcNow.AddDays(3));

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager
        };

        var handler = new GetBookingsForBranchQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db)
        );

        var query = new GetBookingsForBranchQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Equal(branch1.Id, b.BranchId));
        Assert.Contains(result, b => b.VehiclePlateNumber == "ABC123");
        Assert.Contains(result, b => b.VehiclePlateNumber == "XYZ789");
        Assert.DoesNotContain(result, b => b.VehiclePlateNumber == "DEF456");
    }

    [Fact]
    public async Task GetBookingsForBranch_Should_ReturnEmpty_WhenNoBookingsExist()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        var branch = ShopOwnerSeed.Branch(db, company, "Branch1");
        var manager = ShopOwnerSeed.ShopManager(db, branch, "Manager1", "manager1@test.com");

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager
        };

        var handler = new GetBookingsForBranchQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db)
        );

        var query = new GetBookingsForBranchQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBookingsForBranch_Should_Throw_WhenNotAuthenticated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = false
        };

        var handler = new GetBookingsForBranchQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db)
        );

        var query = new GetBookingsForBranchQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetBookingsForBranch_Should_Throw_WhenNotShopManager()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        var branch = ShopOwnerSeed.Branch(db, company, "Branch1");

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = ownerId,
            Role = UserRole.ShopOwner // Not a ShopManager
        };

        var handler = new GetBookingsForBranchQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db)
        );

        var query = new GetBookingsForBranchQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetBookingsForBranch_Should_Throw_WhenManagerNotAssignedToBranch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        // Create a shop manager without branch assignment
        var manager = new DomainLayer.Users.User("Manager1", "manager1@test.com", null, UserRole.ShopManager);
        manager.SetPasswordHash("hashedpassword");
        db.Users.Add(manager);

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager
        };

        var handler = new GetBookingsForBranchQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db)
        );

        var query = new GetBookingsForBranchQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetBookingsForBranch_Should_OrderByAppointmentDate()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        var branch = ShopOwnerSeed.Branch(db, company, "Branch1");
        var manager = ShopOwnerSeed.ShopManager(db, branch, "Manager1", "manager1@test.com");

        var vehicleOwner = Guid.NewGuid();
        var vehicle1 = ShopOwnerSeed.Vehicle(db, vehicleOwner, "ABC123");
        var vehicle2 = ShopOwnerSeed.Vehicle(db, vehicleOwner, "XYZ789");
        var vehicle3 = ShopOwnerSeed.Vehicle(db, vehicleOwner, "DEF456");

        // Create bookings with different dates (not in order)
        var booking1 = ShopOwnerSeed.Booking(db, branch, vehicle1, ServiceType.ChangeTires, DateTime.UtcNow.AddDays(5));
        var booking2 = ShopOwnerSeed.Booking(db, branch, vehicle2, ServiceType.BuyNewTires, DateTime.UtcNow.AddDays(1));
        var booking3 = ShopOwnerSeed.Booking(db, branch, vehicle3, ServiceType.ChangeTires, DateTime.UtcNow.AddDays(3));

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager
        };

        var handler = new GetBookingsForBranchQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db)
        );

        var query = new GetBookingsForBranchQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("XYZ789", result[0].VehiclePlateNumber); // Day 1
        Assert.Equal("DEF456", result[1].VehiclePlateNumber); // Day 3
        Assert.Equal("ABC123", result[2].VehiclePlateNumber); // Day 5
    }
}
