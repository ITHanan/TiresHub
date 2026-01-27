using ApplicationLayer.Features.TireSet.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TireSet.Queries
{
    public class GetVehicleTireSetsQueryHandler : IRequestHandler<GetVehicleTireSetsQuery, OperationResult<List<TireSetDto>>>
    {
        private readonly IVehicleRepository _vehicles;
        private readonly ITireSetRepository _tireSets;

        public GetVehicleTireSetsQueryHandler(IVehicleRepository vehicles, ITireSetRepository tireSets)
        {
            _vehicles = vehicles;
            _tireSets = tireSets;
        }

        public async Task<OperationResult<List<TireSetDto>>> Handle(GetVehicleTireSetsQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicles.GetByIdAsync(request.VehicleId);
            if (vehicle is null)
                return OperationResult<List<TireSetDto>>
                    .Failure("Vehicle not found.");

            if (vehicle.OwnerId != request.OwnerId)
                return OperationResult<List<TireSetDto>>
                    .Failure("You do not have access to this vehicle.");

            var tireSets = await _tireSets
                .ListByVehicleAsync(request.VehicleId);

            var dtoList = tireSets
                .Select(t => new TireSetDto(
                    Id: t.Id,
                    VehicleId: t.VehicleId,
                    TireType: t.TireType.ToString(),
                    Size: t.Size,
                    Brand: t.Brand,
                    Notes: t.Notes,
                    IsLocked: t.IsLocked,
                    CreatedAt: t.CreatedAt
                ))
                .ToList();

            return OperationResult<List<TireSetDto>>.Success(dtoList);
        }
    }
}
