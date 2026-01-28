using ApplicationLayer.Features.TireSet.Command.UpdateTireSet;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Vehicles;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.TireSets
{
    public class UpdateTireSetCommandHandlerTestsAfterServiceAndBeforeService
    {
        private readonly Mock<IVehicleRepository> _vehicleRepo = new();
        private readonly Mock<ITireSetRepository> _tireSetRepo = new();
        private readonly Mock<IAuditRepository> _auditRepo = new();

        private UpdateTireSetCommandHandler CreateHandler()
            => new(_vehicleRepo.Object, _tireSetRepo.Object, _auditRepo.Object);


        private Vehicle CreateVehicle(Guid ownerId, bool completedService)
        {
            var vehicle = new Vehicle("ABC123", ownerId);
            typeof(Vehicle).GetProperty("Id")!.SetValue(vehicle, Guid.NewGuid());
            typeof(Vehicle).GetProperty("HasCompletedService")!
                .SetValue(vehicle, completedService);
            return vehicle;
        }

        private TireSet CreateTireSet(Guid vehicleId)
        {
            var tireSet = new TireSet(
                vehicleId,
                TireType.Summer,
                "205/55R16",
                "Michelin",
                null);

            typeof(TireSet).GetProperty("Id")!
                .SetValue(tireSet, Guid.NewGuid());

            return tireSet;
        }

        [Fact]
        public async Task Owner_Can_Update_TireSet_Before_Service()
        {
            var ownerId = Guid.NewGuid();
            var vehicle = CreateVehicle(ownerId, completedService: false);
            var tireSet = CreateTireSet(vehicle.Id);

            _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
            _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new UpdateTireSetCommand(
                    ownerId,
                    UserRole.VehicleOwner,
                    tireSet.Id,
                    "225/45R17",
                    "Pirelli",
                    "Updated"),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Manager_Can_Update_TireSet_After_Service()
        {
            var ownerId = Guid.NewGuid();
            var managerId = Guid.NewGuid();
            var vehicle = CreateVehicle(ownerId, completedService: true);
            var tireSet = CreateTireSet(vehicle.Id);

            _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
            _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new UpdateTireSetCommand(
                    managerId,
                    UserRole.ShopManager,
                    tireSet.Id,
                    "225/45R17",
                    "Continental",
                    "Manager update"),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }


        [Fact]
        public async Task Manager_Cannot_Update_TireSet_Before_Service()
        {
            var ownerId = Guid.NewGuid();
            var managerId = Guid.NewGuid();
            var vehicle = CreateVehicle(ownerId, completedService: false);
            var tireSet = CreateTireSet(vehicle.Id);

            _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
            _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new UpdateTireSetCommand(
                    managerId,
                    UserRole.ShopManager,
                    tireSet.Id,
                    "225/45R17",
                    "Pirelli",
                    null),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("owner");
        }


        [Fact]
        public async Task Owner_Cannot_Update_TireSet_After_Service()
        {
            var ownerId = Guid.NewGuid();
            var vehicle = CreateVehicle(ownerId, completedService: true);
            var tireSet = CreateTireSet(vehicle.Id);

            _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
            _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new UpdateTireSetCommand(
                    ownerId,
                    UserRole.VehicleOwner,
                    tireSet.Id,
                    "225/45R17",
                    "Pirelli",
                    null),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("locked");
        }

        [Fact]
        public async Task Update_Fails_When_Size_Is_Missing()
        {
            var ownerId = Guid.NewGuid();
            var vehicle = CreateVehicle(ownerId, false);
            var tireSet = CreateTireSet(vehicle.Id);

            _tireSetRepo.Setup(r => r.GetByIdAsync(tireSet.Id)).ReturnsAsync(tireSet);
            _vehicleRepo.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new UpdateTireSetCommand(
                    ownerId,
                    UserRole.VehicleOwner,
                    tireSet.Id,
                    "",
                    "Michelin",
                    null),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("size");
        }


    }


}
