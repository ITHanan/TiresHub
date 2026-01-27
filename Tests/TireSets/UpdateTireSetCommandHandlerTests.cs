using ApplicationLayer.Features.TireSet.Command.UpdateTireSet;
using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Common;
using DomainLayer.Enums;
using DomainLayer.Vehicles;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests;

public class UpdateTireSetCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepo = new();
    private readonly Mock<ITireSetRepository> _tireSetRepo = new();
    private readonly Mock<IAuditRepository> _auditRepo = new();

    private UpdateTireSetCommandHandler CreateHandler()
        => new(_vehicleRepo.Object, _tireSetRepo.Object, _auditRepo.Object);

    // ---------- Helpers ----------
    private static Vehicle CreateVehicle(Guid ownerId, bool hasCompletedService)
    {
        var v = new Vehicle(
            plateNumber: "ABC123",
            ownerId: ownerId,
            make: null,
            model: null,
            year: null
        );

        typeof(Vehicle).GetProperty("Id")!.SetValue(v, Guid.NewGuid());

        typeof(Vehicle).GetProperty("HasCompletedService")!
            .SetValue(v, hasCompletedService);

        return v;
    }

    private static DomainLayer.Vehicles.TireSet CreateTireSet(Guid vehicleId)
    {
        var ts = new DomainLayer.Vehicles.TireSet(
            vehicleId: vehicleId,
            tireType: TireType.Summer,
            size: "205/55R16",
            brand: "Michelin",
            notes: null
        );

        typeof(DomainLayer.Vehicles.TireSet).GetProperty("Id")!
            .SetValue(ts, Guid.NewGuid());

        return ts;
    }

    // ---------- Tests ----------

    [Fact]
    public async Task Update_Fails_When_TireSet_NotFound()
    {
        // Arrange
        var handler = CreateHandler();

        _tireSetRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((DomainLayer.Vehicles.TireSet?)null);

        var cmd = new UpdateTireSetCommand(
            ActorUserId: Guid.NewGuid(),
            ActorRole: UserRole.ShopManager,
            TireSetId: Guid.NewGuid(),
            Size: "225/45R17",
            Brand: "Pirelli",
            Notes: "Updated"
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Tire set not found.");

        _auditRepo.Verify(a => a.LogAsync(
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()
        ), Times.Never);
    }

    [Fact]
    public async Task Update_Fails_When_Vehicle_NotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var tireSet = CreateTireSet(vehicleId);

        _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync((Vehicle?)null);

        var handler = CreateHandler();

        var cmd = new UpdateTireSetCommand(
            ActorUserId: Guid.NewGuid(),
            ActorRole: UserRole.ShopManager,
            TireSetId: tireSet.Id,
            Size: "225/45R17",
            Brand: "Pirelli",
            Notes: "Updated"
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Vehicle not found.");

        _auditRepo.Verify(a => a.LogAsync(
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()
        ), Times.Never);
    }

    [Fact]
    public async Task Update_Fails_When_Service_NotCompleted()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicle = CreateVehicle(ownerId, hasCompletedService: false);
        var tireSet = CreateTireSet(vehicle.Id);

        _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var cmd = new UpdateTireSetCommand(
            ActorUserId: Guid.NewGuid(),
            ActorRole: UserRole.ShopManager,
            TireSetId: tireSet.Id,
            Size: "225/45R17",
            Brand: "Pirelli",
            Notes: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("only be updated after service completion");

        _auditRepo.Verify(a => a.LogAsync(
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()
        ), Times.Never);

        _tireSetRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_Fails_When_Actor_Is_VehicleOwner_After_Service_And_Logs_AuditFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicle = CreateVehicle(ownerId, hasCompletedService: true);
        var tireSet = CreateTireSet(vehicle.Id);

        _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var cmd = new UpdateTireSetCommand(
            ActorUserId: ownerId,
            ActorRole: UserRole.VehicleOwner,
            TireSetId: tireSet.Id,
            Size: "225/45R17",
            Brand: "Pirelli",
            Notes: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("locked");

        _auditRepo.Verify(a => a.LogAsync(
            ownerId,
            AuditActions.TireSetUpdateFailed,
            nameof(DomainLayer.Vehicles.TireSet),
            tireSet.Id,
            false,
            It.Is<string>(s => s.Contains("locked", StringComparison.OrdinalIgnoreCase)
                            || s.Contains("locked after service", StringComparison.OrdinalIgnoreCase)),
            null
        ), Times.Once);

        _tireSetRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_Fails_When_Actor_Is_Not_ShopManager_After_Service()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicle = CreateVehicle(ownerId, hasCompletedService: true);
        var tireSet = CreateTireSet(vehicle.Id);

        _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var cmd = new UpdateTireSetCommand(
            ActorUserId: Guid.NewGuid(),
            ActorRole: UserRole.Employee,
            TireSetId: tireSet.Id,
            Size: "225/45R17",
            Brand: "Pirelli",
            Notes: null
        );




        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Unauthorized.");

        _auditRepo.Verify(a => a.LogAsync(
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()
        ), Times.Never);

        _tireSetRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_Succeeds_For_ShopManager_After_Service_And_Logs_AuditSuccess()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var vehicle = CreateVehicle(ownerId, hasCompletedService: true);
        var tireSet = CreateTireSet(vehicle.Id);

        _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var cmd = new UpdateTireSetCommand(
            ActorUserId: managerId,
            ActorRole: UserRole.ShopManager,
            TireSetId: tireSet.Id,
            Size: "225/45R17",
            Brand: "Pirelli",
            Notes: "Updated"
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        _tireSetRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        _auditRepo.Verify(a => a.LogAsync(
            managerId,
            AuditActions.TireSetUpdated,
            nameof(DomainLayer.Vehicles.TireSet),
            tireSet.Id,
            true,
            null,
            null
        ), Times.Once);
    }

    [Fact]
    public async Task Update_Fails_When_DomainValidation_Throws_ArgumentException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var vehicle = CreateVehicle(ownerId, hasCompletedService: true);
        var tireSet = CreateTireSet(vehicle.Id);

        _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var cmd = new UpdateTireSetCommand(
            ActorUserId: managerId,
            ActorRole: UserRole.ShopManager,
            TireSetId: tireSet.Id,
            Size: "",
            Brand: "Pirelli",
            Notes: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();

        _tireSetRepo.Verify(r => r.SaveChangesAsync(), Times.Never);

        _auditRepo.Verify(a => a.LogAsync(
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()
        ), Times.Never);
    }


    [Fact]
    public async Task Update_Fails_When_Size_Is_Empty()
    {
        var ownerId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var vehicle = CreateVehicle(ownerId, true);
        var tireSet = CreateTireSet(vehicle.Id);

        _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id))
            .ReturnsAsync(tireSet);

        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id))
            .ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new UpdateTireSetCommand(
                managerId,
                UserRole.ShopManager,
                tireSet.Id,
                "",
                "Pirelli",
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Tire size is required");
    }



}
