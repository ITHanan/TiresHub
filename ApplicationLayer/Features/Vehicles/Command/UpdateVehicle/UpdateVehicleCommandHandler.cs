using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Command.UpdateVehicle
{
    public class UpdateVehicleCommandHandler
        : IRequestHandler<UpdateVehicleCommand, OperationResult<bool>>
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IAuditRepository _audit;

        public UpdateVehicleCommandHandler(
            IVehicleRepository vehicles,
            IAuditRepository audit)
        {
            _vehicles = vehicles;
            _audit = audit;
        }

        public async Task<OperationResult<bool>> Handle(
            UpdateVehicleCommand request,
            CancellationToken cancellationToken)
        {
            var vehicle = await _vehicles.GetByIdAsync(request.VehicleId);

            if (vehicle is null)
            {
                await LogFail(request, "Vehicle not found.");
                return OperationResult<bool>.Failure("Vehicle not found.");
            }

            if (vehicle.OwnerId != request.OwnerId)
            {
                await LogFail(request, "Unauthorized vehicle access.");
                return OperationResult<bool>.Failure("You do not have access to this vehicle.");
            }

            if (!vehicle.IsActive)
            {
                return OperationResult<bool>.Failure(
                    "Inactive vehicles cannot be updated.");
            }

            if (vehicle.HasCompletedService)
            {
                return OperationResult<bool>.Failure(
                    "Vehicle cannot be updated after service completion.");
            }

            try
            {
                // Update allowed fields only
                typeof(DomainLayer.Vehicles.Vehicle)
                    .GetProperty("Make")?
                    .SetValue(vehicle, request.Make);

                typeof(DomainLayer.Vehicles.Vehicle)
                    .GetProperty("Model")?
                    .SetValue(vehicle, request.Model);

                typeof(DomainLayer.Vehicles.Vehicle)
                    .GetMethod("SetYear", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .Invoke(vehicle, new object?[] { request.Year });
            }
            catch (Exception ex)
            {
                await LogFail(request, ex.Message);
                return OperationResult<bool>.Failure(ex.Message);
            }

            await _vehicles.SaveChangesAsync();

            await _audit.LogAsync(
                request.OwnerId,
                AuditActions.VehicleUpdated,
                nameof(DomainLayer.Vehicles.Vehicle),
                vehicle.Id,
                true,
                null,
                null);

            return OperationResult<bool>.Success(true);
        }

        private Task LogFail(UpdateVehicleCommand request, string reason)
        {
            return _audit.LogAsync(
                request.OwnerId,
                AuditActions.VehicleUpdateFailed,
                nameof(DomainLayer.Vehicles.Vehicle),
                request.VehicleId,
                false,
                reason,
                null);
        }
    }
}
