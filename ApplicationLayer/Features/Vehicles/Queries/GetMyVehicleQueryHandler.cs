using ApplicationLayer.Features.Vehicles.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Queries
{
    public class GetMyVehiclesQueryHandler
       : IRequestHandler<GetMyVehiclesQuery, OperationResult<List<VehicleDto>>>
    {
        private readonly IVehicleRepository _vehicles;

        public GetMyVehiclesQueryHandler(IVehicleRepository vehicles)
        {
            _vehicles = vehicles;
        }

        public async Task<OperationResult<List<VehicleDto>>> Handle(
            GetMyVehiclesQuery request,
            CancellationToken cancellationToken)
        {
            var vehicles = await _vehicles.GetActiveByOwnerAsync(request.OwnerId);


           // Find only active vehicles
            var activeVehicle = vehicles.Where(v => v.IsActive).Select(v => new VehicleDto(
                v.Id,
                v.PlateNumber,
                v.Make,
                v.Model,
                v.Year,
                v.CreatedAt
            )).ToList();

            return OperationResult<List<VehicleDto>>.Success(activeVehicle);
        }
    }
}
