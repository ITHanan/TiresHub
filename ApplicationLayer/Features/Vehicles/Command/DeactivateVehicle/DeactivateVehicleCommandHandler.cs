using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Command.DeactivateVehicle
{
    public class DeactivateVehicleCommandHandler
        : IRequestHandler<DeactivateVehicleCommand, OperationResult<bool>>
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IAuditRepository _audit;

        public DeactivateVehicleCommandHandler(
            IVehicleRepository vehicles,
            IAuditRepository audit)
        {
            _vehicles = vehicles;
            _audit = audit;
        }

        public async Task<OperationResult<bool>> Handle(DeactivateVehicleCommand request, CancellationToken cancellationToken)
        {
            // 1) Load vehicle
            var vehicle = await _vehicles.GetByIdAsync(request.VehicleId);

            if (vehicle is null)
            {
                await LogFail(request, "Vehicle not found.");
                return OperationResult<bool>.Failure("Vehicle not found.");
            }

            // 2) Ownership check
            if (vehicle.OwnerId != request.OwnerId)
            {
                await LogFail(request, "Unauthorized vehicle access.");
                return OperationResult<bool>.Failure("You do not have access to this vehicle.");
            }

            // 3) If already inactive (graceful)
            if (!vehicle.IsActive)
            {
                await _audit.LogAsync(
                    request.OwnerId,
                    AuditActions.VehicleDeactivated,
                    nameof(Vehicle),
                    vehicle.Id,
                    true,
                    "Vehicle already inactive.",
                    null);

                return OperationResult<bool>.Success(true);
            }

            // 4) Deactivate
            vehicle.Deactivate();

            // 5) Save
            await _vehicles.SaveChangesAsync();

            // 6) Audit success
            await _audit.LogAsync(
                request.OwnerId,
                AuditActions.VehicleDeactivated,
                nameof(Vehicle),
                vehicle.Id,
                true,
                null,
                null);

            return OperationResult<bool>.Success(true);




        }

        private Task LogFail(DeactivateVehicleCommand request, string reason)
        {
            return _audit.LogAsync(
                request.OwnerId,
                AuditActions.VehicleDeactivationFailed,
                nameof(Vehicle),
                request.VehicleId,
                false,
                reason,
                null);
        }
    }

}