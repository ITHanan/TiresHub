using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Common;
using MediatR;

namespace ApplicationLayer.Features.Vehicles.Command.CreateVehicle
{
    public class CreateVehicleCommandHandler
     : IRequestHandler<CreateVehicleCommand, OperationResult<Guid>>
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IAuditRepository _audit;

        public CreateVehicleCommandHandler(
            IVehicleRepository vehicles,
            IAuditRepository audit)
        {
            _vehicles = vehicles;
            _audit = audit;
        }

        public async Task<OperationResult<Guid>> Handle(
            CreateVehicleCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PlateNumber))
            {
                await _audit.LogAsync(
                    request.OwnerId,
                    AuditActions.VehicleCreateFailed,
                    nameof(Vehicle),
                    null,
                    success: false,
                    reason: "Plate number is required",
                    null
                );

                return OperationResult<Guid>.Failure("License plate is required.");
            }

            if (request.Year.HasValue &&
                (request.Year < 1900 || request.Year > DateTime.UtcNow.Year + 1))
            {
                await _audit.LogAsync(
                    request.OwnerId,
                    AuditActions.VehicleCreateFailed,
                    nameof(Vehicle),
                    null,
                    false,
                    "Invalid vehicle year",
                    null
                );

                return OperationResult<Guid>.Failure("Invalid vehicle year.");
            }

            // 2️⃣ Normalize plate
            var plate = request.PlateNumber.Trim().ToUpperInvariant();

            // 3️⃣ Duplicate check
            if (await _vehicles.ExistsAsync(request.OwnerId, plate))
            {
                await _audit.LogAsync(
                    request.OwnerId,
                    AuditActions.VehicleCreateFailed,
                    nameof(Vehicle),
                    null,
                    false,
                    "Duplicate vehicle",
                    null
                );

                return OperationResult<Guid>.Failure(
                    "This vehicle is already registered.");
            }

            // 4️⃣ Create vehicle
            var vehicle = new DomainLayer.Vehicles.Vehicle(
                plate,
                request.OwnerId,
                request.Make,
                request.Model,
                request.Year
            );

            await _vehicles.AddAsync(vehicle);
            await _vehicles.SaveChangesAsync();

            // 5️⃣ Audit success
            await _audit.LogAsync(
                request.OwnerId,
                AuditActions.VehicleCreated,
                nameof(Vehicle),
                vehicle.Id,
                true,
                null,
                null
            );

            return OperationResult<Guid>.Success(vehicle.Id);
        }
    }

}
