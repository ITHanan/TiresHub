using ApplicationLayer.Features.Vehicle.Command;
using ApplicationLayer.Features.Vehicles.Command;
using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Vehicles;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests;

public class CreateVehicleCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepo = new();
    private readonly Mock<IAuditRepository> _auditRepo = new();

    private CreateVehicleCommandHandler CreateHandler()
    {
        return new CreateVehicleCommandHandler(_vehicleRepo.Object, _auditRepo.Object);
    }

    //1. Register vehicle successfully
    [Fact]
    public async Task CreateVehicle_Succeeds_With_PlateOnly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        _vehicleRepo
            .Setup(r => r.ExistsAsync(ownerId, "ABC123"))
            .ReturnsAsync(false);

        var command = new CreateVehicleCommand(
            ownerId,
            "ABC123",
            null,
            null,
            null
        );

        var handler = CreateHandler();



        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _auditRepo.Verify(a => a.LogAsync(
            ownerId,
            AuditActions.VehicleCreated,
            nameof(Vehicle),
            It.IsAny<Guid>(),
            true,
            null,
            null
        ), Times.Once);

        result.Data.Should().NotBeEmpty();

        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
        _vehicleRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

    }

    //2. Fail to register duplicate vehicle
    [Fact]
    public async Task CreateVehicle_Fails_For_Duplicate_Plate()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _vehicleRepo
            .Setup(r => r.ExistsAsync(ownerId, "ABC123"))
            .ReturnsAsync(true);
        var command = new CreateVehicleCommand(
            ownerId,
            "ABC123",
            null,
            null,
            null
        );
        var handler = CreateHandler();


        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeFalse();

        _auditRepo.Verify(a => a.LogAsync(
           ownerId,
           AuditActions.VehicleCreateFailed,
           nameof(Vehicle),
           null,
           false,
           It.IsAny<string>(),
           null
       ), Times.Once);

        result.ErrorMessage.Should().Be("This vehicle is already registered.");
        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Never);
        _vehicleRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    //3.Register with optional fields
    [Fact]
    public async Task CreateVehicle_Succeeds_With_All_Fields()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _vehicleRepo
            .Setup(r => r.ExistsAsync(ownerId, "XYZ789"))
            .ReturnsAsync(false);
        var command = new CreateVehicleCommand(
            ownerId,
            "XYZ789",
            "Toyota",
            "Camry",
            2020
        );
        var handler = CreateHandler();



        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeTrue();

        _auditRepo.Verify(a => a.LogAsync(
           ownerId,
           AuditActions.VehicleCreated,
           nameof(Vehicle),
           It.IsAny<Guid>(),
           true,
           null,
           null
       ), Times.Once);

        result.Data.Should().NotBeEmpty();
        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
        _vehicleRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // 4. Trim and uppercase plate number
    [Fact]
    public async Task CreateVehicle_Trims_And_Uppercases_PlateNumber()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _vehicleRepo
            .Setup(r => r.ExistsAsync(ownerId, "LMN456"))
            .ReturnsAsync(false);
        Vehicle? addedVehicle = null;
        _vehicleRepo
            .Setup(r => r.AddAsync(It.IsAny<Vehicle>()))
            .Callback<Vehicle>(v => addedVehicle = v)
            .Returns(Task.CompletedTask);
        var command = new CreateVehicleCommand(
            ownerId,
            "  lmn456  ",
            null,
            null,
            null
        );
        var handler = CreateHandler();
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        addedVehicle.Should().NotBeNull();
        addedVehicle!.PlateNumber.Should().Be("LMN456");
        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
        _vehicleRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // 5.Reject missing plate (Validator)

    [Fact]
    public async Task CreateVehicle_Fails_When_PlateNumber_Is_Missing()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateVehicleCommand(
            ownerId,
            "",
            null,
            null,
            null
        );
        var handler = CreateHandler();


        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeFalse();

        _auditRepo.Verify(a => a.LogAsync(
          ownerId,
          AuditActions.VehicleCreateFailed,
          nameof(Vehicle),
          null,
          false,
          It.IsAny<string>(),
          null
      ), Times.Once);

        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Never);
        _vehicleRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    //6. Reject invalid year (Validator)
    [Fact]
    public async Task CreateVehicle_Fails_When_Year_Is_Invalid()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateVehicleCommand(
            ownerId,
            "VALID123",
            null,
            null,
            1800 // Invalid year
        );
        var handler = CreateHandler();


        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeFalse();

        _auditRepo.Verify(a => a.LogAsync(
          ownerId,
          AuditActions.VehicleCreateFailed,
          nameof(Vehicle),
          null,
          false,
          It.IsAny<string>(),
          null
      ), Times.Once);

        _vehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Never);
        _vehicleRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    

}