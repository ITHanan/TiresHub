using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TireSet.Command.UpdateTireSet
{
    public class UpdateTireSetCommandHandler : IRequestHandler<UpdateTireSetCommand, OperationResult<bool>>
    {
        private readonly IVehicleRepository _vehicles;
        private readonly ITireSetRepository _tireSets;
        private readonly IAuditRepository _audit;

        public UpdateTireSetCommandHandler(
            IVehicleRepository vehicles,
            ITireSetRepository tireSets,
            IAuditRepository audit)
        {
            _vehicles = vehicles;
            _tireSets = tireSets;
            _audit = audit;
        }
        public async Task<OperationResult<bool>> Handle(UpdateTireSetCommand request, CancellationToken cancellationToken)
        {
            var tireSet = await _tireSets.GetByIdAsync(request.TireSetId);
            if (tireSet is null)
                return OperationResult<bool>.Failure("Tire set not found.");

            var vehicle = await _vehicles.GetByIdAsync(tireSet.VehicleId);
            if (vehicle is null)
                return OperationResult<bool>.Failure("Vehicle not found.");

            if (vehicle.HasCompletedService)
            {
                if (request.ActorRole == UserRole.VehicleOwner)
                {
                    await LogFail(request, "Tire data locked after service completion.");
                    return OperationResult<bool>.Failure(
                        "Tire data is locked after service completion.");
                }

                if (request.ActorRole != UserRole.ShopManager)
                    return OperationResult<bool>.Failure("Unauthorized.");
            }
            else
            {
                return OperationResult<bool>.Failure(
                    "Tire data can only be updated after service completion.");
            }

            try
            {
                tireSet.Update(request.Size, request.Brand, request.Notes);
            }
            catch (ArgumentException ex)
            {
                return OperationResult<bool>.Failure(ex.Message);
            }

            await _tireSets.SaveChangesAsync();

            await _audit.LogAsync(
                request.ActorUserId,
                AuditActions.TireSetUpdated,
                nameof(TireSet),
                tireSet.Id,
                true,
                null,
                null);

            return OperationResult<bool>.Success(true);
        }

        private Task LogFail(UpdateTireSetCommand request, string reason)
        {
            return _audit.LogAsync(
                request.ActorUserId,
                AuditActions.TireSetUpdateFailed,
                nameof(TireSet),
                request.TireSetId,
                false,
                reason,
                null);
        }
    }
}

