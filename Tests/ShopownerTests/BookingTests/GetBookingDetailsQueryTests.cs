using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails;
using DomainLayer.Enums;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.BookingTests;

public class GetBookingDetailsQueryTests
{
    [Fact]
    public async Task GetBookingDetails_Should_Return_DetailsForAssignedBranch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        var branch = ShopOwnerSeed.Branch(db, company, "Branch1", "TestCity", "123 Main St");
        var manager = ShopOwnerSeed.ShopManager(db, branch, "Manager1", "manager1@test.com");

        var vehicleOwner = Guid.NewGuid();
        var vehicle = ShopOwnerSeed.Vehicle(db, vehicleOwner, "ABC123");
        var booking = ShopOwnerSeed.Booking(db, branch, vehicle, ServiceType.ChangeTires, DateTime.UtcNow.AddDays(1));

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager
        };

        var handler = new GetBookingDetailsQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db),
            new AuditRepository(db)
        );

        var query = new GetBookingDetailsQuery(booking.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal("ABC123", result.VehiclePlateNumber);
        Assert.Equal(ServiceType.ChangeTires, result.ServiceType);
        Assert.Equal(branch.Id, result.BranchId);
        Assert.Equal("Branch1", result.BranchName);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
    }

    [Fact]
    public async Task GetBookingDetails_Should_Throw_WhenBookingFromDifferentBranch()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        var branch1 = ShopOwnerSeed.Branch(db, company, "Branch1");
        var branch2 = ShopOwnerSeed.Branch(db, company, "Branch2");

        var manager = ShopOwnerSeed.ShopManager(db, branch1, "Manager1", "manager1@test.com");

        var vehicleOwner = Guid.NewGuid();
        var vehicle = ShopOwnerSeed.Vehicle(db, vehicleOwner, "ABC123");

        // Create booking for branch2
        var booking = ShopOwnerSeed.Booking(db, branch2, vehicle, ServiceType.ChangeTires, DateTime.UtcNow.AddDays(1));

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = true,
            UserId = manager.Id,
            Role = UserRole.ShopManager
        };

        var handler = new GetBookingDetailsQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db),
            new AuditRepository(db)
        );

        var query = new GetBookingDetailsQuery(booking.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));

        Assert.Contains("You do not have access to this booking", exception.Message);

        // Verify audit log was created
        var auditLog = db.AuditLogs.FirstOrDefault(a => a.Action == "UnauthorizedBookingAccess");
        Assert.NotNull(auditLog);
        Assert.Equal(manager.Id, auditLog.ActorUserId);
        Assert.Equal(booking.Id, auditLog.EntityId);
    }

    [Fact]
    public async Task GetBookingDetails_Should_Throw_WhenBookingNotFound()
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

        var handler = new GetBookingDetailsQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db),
            new AuditRepository(db)
        );

        var query = new GetBookingDetailsQuery(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetBookingDetails_Should_Throw_WhenNotAuthenticated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = false
        };

        var handler = new GetBookingDetailsQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db),
            new AuditRepository(db)
        );

        var query = new GetBookingDetailsQuery(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetBookingDetails_Should_Throw_WhenNotShopManager()
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
            Role = UserRole.VehicleOwner // Not a ShopManager
        };

        var handler = new GetBookingDetailsQueryHandler(
            currentUser,
            new BookingRepository(db),
            new UserRepository(db),
            new AuditRepository(db)
        );

        var query = new GetBookingDetailsQuery(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));
    }
}
