using ApplicationLayer.Features.Bookings.Commands.AssignWarehouse;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.shops;
using FluentAssertions;
using Moq;
using System.Threading;
using Xunit;

namespace Tests.ShopownerTests.BookingTests;

public class AssignWarehouseCommandHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepo = new();
    private readonly Mock<IWarehouseRepository> _warehouseRepo = new();
    private readonly Mock<ICompanyRepository> _companyRepo = new();
    private readonly Mock<IBranchRepository> _branchRepo = new();
    private readonly Mock<IAuditRepository> _auditRepo = new();

    private AssignWarehouseCommandHandler CreateHandler()
        => new(
            _bookingRepo.Object,
            _warehouse_repo_object(),
            _companyRepo.Object,
            _branchRepo.Object,
            _auditRepo.Object
        );

    // small indirection to keep edit minimal; resolves to _warehouseRepo.Object
    private IWarehouseRepository _warehouse_repo_object() => _warehouseRepo.Object;

    [Fact]
    public async Task Assign_available_warehouse_successfully()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid(); // Booking aggregate id used in the test
        var warehouseId = Guid.NewGuid(); // Warehouse aggregate id used in the test
        var vehicleId = Guid.NewGuid(); // Vehicle id for the booking
        var actorUserId = Guid.NewGuid(); // The user performing the assignment (shop manager/owner)

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            branchId,
            DomainLayer.Enums.TireType.Summer,
            null
        );
        // Using reflection to set the persisted Id on the aggregate for assertion purposes
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        // Give the warehouse a clear name for readability in tests (e.g. "Main Warehouse")
        var warehouse = new Warehouse("Main Warehouse", capacity: 2, branchId: branchId);
        typeof(Warehouse).GetProperty("Id")!.SetValue(warehouse, warehouseId);

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _warehouseRepo
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _companyRepo
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignWarehouseCommand(actorUserId, UserRole.ShopManager, bookingId, warehouseId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.WarehouseId.Should().Be(warehouseId);
        warehouse.CurrentUsage.Should().Be(1);

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditRepo.Verify(a => a.LogAsync(
            actorUserId,
            It.IsAny<string>(),
            nameof(Booking),
            bookingId,
            true,
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task Reject_full_warehouse_assignment()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            branchId,
            DomainLayer.Enums.TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        var warehouse = new Warehouse("Main Warehouse", capacity: 1, branchId: branchId);
        typeof(Warehouse).GetProperty("Id")!.SetValue(warehouse, warehouseId);
        // Fill the warehouse so it becomes full
        warehouse.IncreaseUsage();

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _warehouseRepo
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignWarehouseCommand(Guid.NewGuid(), UserRole.ShopManager, bookingId, warehouseId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("full");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditRepo.Verify(a => a.LogAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        booking.WarehouseId.Should().BeNull();
    }

    [Fact]
    public async Task Reject_cross_branch_warehouse_assignment()
    {
        // Arrange
        var bookingBranchId = Guid.NewGuid();
        var warehouseBranchId = Guid.NewGuid();

        var bookingId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            bookingBranchId,
            DomainLayer.Enums.TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        var warehouse = new Warehouse("Other Branch Warehouse", capacity: 5, branchId: warehouseBranchId);
        typeof(Warehouse).GetProperty("Id")!.SetValue(warehouse, warehouseId);

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _warehouseRepo
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignWarehouseCommand(Guid.NewGuid(), UserRole.ShopManager, bookingId, warehouseId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("only assign warehouses from your branch");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditRepo.Verify(a => a.LogAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        booking.WarehouseId.Should().BeNull();
    }

    [Fact]
    public async Task Update_capacity_correctly_when_assigning_to_partially_used_warehouse()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            branchId,
            DomainLayer.Enums.TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        var warehouse = new Warehouse("Main Warehouse", capacity: 2, branchId: branchId);
        typeof(Warehouse).GetProperty("Id")!.SetValue(warehouse, warehouseId);
        // already one used
        warehouse.IncreaseUsage();

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _warehouseRepo
            .Setup(r => r.GetByIdAsync(warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _companyRepo
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignWarehouseCommand(Guid.NewGuid(), UserRole.ShopManager, bookingId, warehouseId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        warehouse.CurrentUsage.Should().Be(2);
        warehouse.IsFull().Should().BeTrue();
        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
