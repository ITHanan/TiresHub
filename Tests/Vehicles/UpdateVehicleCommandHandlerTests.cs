using ApplicationLayer.Features.Vehicles.Command.UpdateVehicle;
using ApplicationLayer.Interfaces;
using DomainLayer.Vehicles;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests;

public class UpdateVehicleCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepo = new();
    private readonly Mock<IAuditRepository> _auditRepo = new();

    private UpdateVehicleCommandHandler CreateHandler()
        => new(_vehicleRepo.Object, _auditRepo.Object);

    [Fact]
    public async Task Update_Succeeds_When_Valid_And_Before_Service()
    {
        var ownerId = Guid.NewGuid();
        var vehicle = CreateVehicle(ownerId);

        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id))
            .ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new UpdateVehicleCommand(
                ownerId,
                vehicle.Id,
                "BMW",
                "X5",
                2022),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        vehicle.Make.Should().Be("BMW");
        vehicle.Model.Should().Be("X5");
        vehicle.Year.Should().Be(2022);
    }

    [Fact]
    public async Task Update_Fails_When_Service_Completed()
    {
        var ownerId = Guid.NewGuid();
        var vehicle = CreateVehicle(ownerId);
        vehicle.MarkServiceCompleted();

        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id))
            .ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new UpdateVehicleCommand(
                ownerId,
                vehicle.Id,
                "Audi",
                "A6",
                2021),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("service");
    }
    [Fact]
    public async Task Update_Fails_When_Vehicle_Inactive()
    {
        var ownerId = Guid.NewGuid();
        var vehicle = CreateVehicle(ownerId);
        vehicle.Deactivate();

        _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id))
            .ReturnsAsync(vehicle);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new UpdateVehicleCommand(
                ownerId,
                vehicle.Id,
                "Volvo",
                "XC90",
                2020),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Inactive");
    }

    // -------- helpers --------

    private Vehicle CreateVehicle(Guid ownerId)
    {
        var vehicle = new Vehicle(
            plateNumber: "ABC123",
            ownerId: ownerId,
            make: null,
            model: null,
            year: null);

        typeof(Vehicle)
            .GetProperty("Id")!
            .SetValue(vehicle, Guid.NewGuid());

        return vehicle;
    }
}