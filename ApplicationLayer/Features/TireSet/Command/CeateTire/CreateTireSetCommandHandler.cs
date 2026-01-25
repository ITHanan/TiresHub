using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Common;
using DomainLayer.Enums;
using DomainLayer.Vehicles;
using MediatR;

namespace ApplicationLayer.Features.TireSet.Command.CeateTire
{
    public class CreateTireSetCommandHandler
        : IRequestHandler<CreateTireSetCommand, OperationResult<Guid>>
    {
        private readonly IVehicleRepository _vehicles;
        private readonly ITireSetRepository _tireSets;
        private readonly IAuditRepository _audit;

        public CreateTireSetCommandHandler(
            IVehicleRepository vehicles,
            ITireSetRepository tireSets,
            IAuditRepository audit)
        {
            _vehicles = vehicles;
            _tireSets = tireSets;
            _audit = audit;
        }

        public async Task<OperationResult<Guid>> Handle(
            CreateTireSetCommand request,
            CancellationToken cancellationToken)
        {
            // 1️  Load vehicle + ownership
            var vehicle = await _vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle is null)
            {
                await LogFail(request.OwnerId, "Vehicle not found.");
                return OperationResult<Guid>.Failure("Vehicle not found.");
            }

            if (vehicle.OwnerId != request.OwnerId)
            {
                await LogFail(request.OwnerId, "Unauthorized vehicle access.");
                return OperationResult<Guid>.Failure("You do not have access to this vehicle.");
            }

            if (vehicle.HasCompletedService)
            {
                await _audit.LogAsync(
                    request.OwnerId,
                    AuditActions.TireSetCreateFailed,
                    nameof(TireSet),
                    null,
                    false,
                    "Tire data is locked after service completion.",
                    null);

                return OperationResult<Guid>.Failure(
                    "Tire data is locked after service completion.");
            }

            // 2️ Validate TireType (required)
            if (request.TireType == 0)
            {
                await LogFail(request.OwnerId, "Tire type is required.");
                return OperationResult<Guid>.Failure("Tire type is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Size))
            {
                await LogFail(request.OwnerId, "Tire size is required.");
                return OperationResult<Guid>.Failure("Tire size is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Brand))
            {
                await LogFail(request.OwnerId, "Tire Brand is required.");
                return OperationResult<Guid>.Failure("Tire Brand is required.");
            }

            // 3️ Prevent duplicate tire type per vehicle
            var exists = await _tireSets.ExistsAsync(request.VehicleId, request.TireType);
            if (exists)
            {
                await LogFail(request.OwnerId, "duplicate tire type for vehicle.");
                return OperationResult<Guid>.Failure(
                    "duplicate tire type for this vehicle.");
            }

            // 4️ Create TireSet (Domain validation)
            DomainLayer.Vehicles. TireSet tireSet;
            try
            {
                tireSet = new DomainLayer.Vehicles.TireSet(
                    vehicleId: request.VehicleId,
                    tireType: request.TireType,
                    size: request.Size,
                    brand: request.Brand,
                    notes: request.Notes
                );
            }
            catch (ArgumentException ex)
            {
                await LogFail(request.OwnerId, ex.Message);
                return OperationResult<Guid>.Failure(ex.Message);
            }

            // 5️ Persist
            await _tireSets.AddAsync(tireSet);
            await _tireSets.SaveChangesAsync();

            // 6️ Audit success
            await _audit.LogAsync(
                request.OwnerId,
                AuditActions.TireSetCreated,
                nameof(TireSet),
                tireSet.Id,
                true,
                null,
                null);

            return OperationResult<Guid>.Success(tireSet.Id);
        }

        private Task LogFail(Guid ownerId, string reason)
        {
            return _audit.LogAsync(
                ownerId,
                AuditActions.TireSetCreateFailed,
                nameof(TireSet),
                null,
                false,
                reason,
                null);
        }
    }
}
