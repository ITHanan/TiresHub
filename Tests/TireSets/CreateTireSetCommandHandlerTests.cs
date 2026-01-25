using ApplicationLayer.Features.TireSet.Command.CeateTire;
using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Enums;
using DomainLayer.Vehicles;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests;

public class CreateTireSetCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepo = new();
    private readonly Mock<ITireSetRepository> _tireSetRepo = new();
    private readonly Mock<IAuditRepository> _auditRepo = new();

    private CreateTireSetCommandHandler CreateHandler()
        => new(
            _vehicleRepo.Object,
            _tireSetRepo.Object,
            _auditRepo.Object
        );

    private static Vehicle VehicleOwnedBy(Guid ownerId)
        => new Vehicle("ABC123", ownerId, make: "Toyota", model: "Camry", year: 2020);

    [Fact]
    public async Task Register_Summer_TireSet_Succeeds()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var vehicle = VehicleOwnedBy(ownerId);

        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        _vehicleRepo
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync(vehicle);

        _tireSetRepo
            .Setup(r => r.ExistsAsync(vehicleId, TireType.Summer))
            .ReturnsAsync(false);

        var command = new CreateTireSetCommand(
            OwnerId: ownerId,
            VehicleId: vehicleId,
            TireType: TireType.Summer,
            Size: "205/55R16",
            Brand: "Michelin",
            Notes: "Front tires"
        );

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _tireSetRepo.Verify(r => r.AddAsync(It.IsAny<DomainLayer.Vehicles.TireSet>()), Times.Once);
        _tireSetRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        _auditRepo.Verify(a => a.LogAsync(
            ownerId,
            AuditActions.TireSetCreated,
            nameof(DomainLayer.Vehicles.TireSet),
            It.IsAny<Guid>(),
            true,
            null,
            null
        ), Times.Once);
    }


    [Fact]
    public async Task Register_Fails_When_Duplicate_TireType()
    {
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var vehicle = VehicleOwnedBy(ownerId);
        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(vehicle);
        _tireSetRepo.Setup(r => r.ExistsAsync(vehicleId, TireType.Summer)).ReturnsAsync(true);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateTireSetCommand(ownerId, vehicleId, TireType.Summer, "205/55R16", "Michelin", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("duplicate");

        _tireSetRepo.Verify(r => r.AddAsync(It.IsAny<DomainLayer.Vehicles.TireSet>()), Times.Never);
        _auditRepo.Verify(a => a.LogAsync(
            ownerId,
            AuditActions.TireSetCreateFailed,
            nameof(DomainLayer.Vehicles.TireSet),
            null,
            false,
            It.IsAny<string>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task Register_Fails_When_TireType_Is_Missing()
    {
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var vehicle = VehicleOwnedBy(ownerId);
        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateTireSetCommand(ownerId, vehicleId, (TireType)0, "205/55R16", "Michelin", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Tire type");
    }

    [Fact]
    public async Task Register_Fails_When_TireSize_Is_Missing()
    {
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var vehicle = VehicleOwnedBy(ownerId);
        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync(vehicle);

        _tireSetRepo.Setup(r => r.ExistsAsync(vehicleId, TireType.Summer))
            .ReturnsAsync(false);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateTireSetCommand(
                ownerId,
                vehicleId,
                TireType.Summer,
                null,
                "Michelin",
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Tire size");

        _tireSetRepo.Verify(r => r.AddAsync(It.IsAny<TireSet>()), Times.Never);
    }

    [Fact]
    public async Task Register_Fails_When_TireBrand_Is_Missing()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var vehicle = VehicleOwnedBy(ownerId);
        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        _vehicleRepo
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync(vehicle);

        _tireSetRepo
            .Setup(r => r.ExistsAsync(vehicleId, TireType.Summer))
            .ReturnsAsync(false);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new CreateTireSetCommand(
                ownerId,
                vehicleId,
                TireType.Summer,
                "205/55R16",
                null,          // ❌ Brand missing
                null
            ),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Brand");

        _tireSetRepo.Verify(
            r => r.AddAsync(It.IsAny<DomainLayer.Vehicles.TireSet>()),
            Times.Never);

        _auditRepo.Verify(a => a.LogAsync(
            ownerId,
            AuditActions.TireSetCreateFailed,
            nameof(DomainLayer.Vehicles.TireSet),
            null,
            false,
            It.IsAny<string>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task Register_Succeeds_When_All_Required_Fields_Are_Provided()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var vehicle = VehicleOwnedBy(ownerId);
        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        _vehicleRepo
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync(vehicle);

        _tireSetRepo
            .Setup(r => r.ExistsAsync(vehicleId, TireType.Summer))
            .ReturnsAsync(false);

        _tireSetRepo
            .Setup(r => r.AddAsync(It.IsAny<DomainLayer.Vehicles.TireSet>()))
            .Returns(Task.CompletedTask);

        _tireSetRepo
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new CreateTireSetCommand(
                ownerId,
                vehicleId,
                TireType.Summer,
                "205/55R16",
                "Michelin",
                "Front tires"
            ),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        _tireSetRepo.Verify(
            r => r.AddAsync(It.Is<DomainLayer.Vehicles.TireSet>(t =>
                t.VehicleId == vehicleId &&
                t.TireType == TireType.Summer &&
                t.Size == "205/55R16" &&
                t.Brand == "Michelin"
            )),
            Times.Once);

        _tireSetRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        _auditRepo.Verify(a => a.LogAsync(
            ownerId,
            AuditActions.TireSetCreated,
            nameof(DomainLayer.Vehicles.TireSet),
            It.IsAny<Guid>(),
            true,
            null,
            null
        ), Times.Once);
    }

    [Fact]
    public async Task Register_Fails_When_Service_Is_Completed()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var vehicle = VehicleOwnedBy(ownerId);
        typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, vehicleId);

        //  simulate completed service
        typeof(Vehicle)
            .GetProperty("HasCompletedService")!
            .SetValue(vehicle, true);

        _vehicleRepo
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync(vehicle);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new CreateTireSetCommand(
                ownerId,
                vehicleId,
                TireType.Summer,
                "205/55R16",
                "Michelin",
                null
            ),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("locked");

        _tireSetRepo.Verify(
            r => r.AddAsync(It.IsAny<DomainLayer.Vehicles.TireSet>()),
            Times.Never);

        _auditRepo.Verify(a => a.LogAsync(
            ownerId,
            AuditActions.TireSetCreateFailed,
            nameof(DomainLayer.Vehicles.TireSet),
            null,
            false,
            It.Is<string>(m => m.Contains("locked")),
            null
        ), Times.Once);
    }
    // Ensure locking after service completion          

}
