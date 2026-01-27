using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Command.ActivateVehicle
{
    public class ActivateVehicleCommandHandler
        : IRequestHandler<ActivateVehicleCommand, OperationResult<bool>>
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IAuditRepository _audit;

        public ActivateVehicleCommandHandler(
            IVehicleRepository vehicles,
            IAuditRepository audit)
        {
            _vehicles = vehicles;
            _audit = audit;
        }

        public async Task<OperationResult<bool>> Handle(
            ActivateVehicleCommand request,
            CancellationToken cancellationToken)
        {
            var vehicle = await _vehicles.GetByIdAsync(request.VehicleId);

            if (vehicle is null)
            {
                await _audit.LogAsync(
                    request.OwnerId,
                    AuditActions.VehicleActivateFailed,
                    nameof(DomainLayer.Vehicles.Vehicle),
                    request.VehicleId,
                    success: false,
                    reason: "Vehicle not found.",
                    metadata: null);

                return OperationResult<bool>.Failure("Vehicle not found.");
            }

            if (vehicle.OwnerId != request.OwnerId)
            {
                await _audit.LogAsync(
                    request.OwnerId,
                    AuditActions.VehicleActivateFailed,
                    nameof(DomainLayer.Vehicles.Vehicle),
                    request.VehicleId,
                    success: false,
                    reason: "Unauthorized vehicle access.",
                    metadata: null);

                return OperationResult<bool>.Failure("You do not have access to this vehicle.");
            }

            if (vehicle.IsActive)
            {
                return OperationResult<bool>.Success(true);
            }

            vehicle.Activate();
            await _vehicles.SaveChangesAsync();

            await _audit.LogAsync(
                request.OwnerId,
                AuditActions.VehicleActivated,
                nameof(DomainLayer.Vehicles.Vehicle),
                vehicle.Id,
                success: true,
                reason: null,
                metadata: null);

            return OperationResult<bool>.Success(true);
        }
    }
}
