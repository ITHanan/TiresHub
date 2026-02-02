using ApplicationLayer.Features.Vehicles.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using MediatR;

namespace ApplicationLayer.Features.Vehicles.Queries
{
    public class GetMyVehiclesQueryHandler
      : IRequestHandler<GetMyVehiclesQuery, OperationResult<MyVehiclesResultDto>>

    {
        private readonly IVehicleRepository _vehicles;

        public GetMyVehiclesQueryHandler(IVehicleRepository vehicles)
        {
            _vehicles = vehicles;
        }

        public async Task<OperationResult<MyVehiclesResultDto>> Handle(
            GetMyVehiclesQuery request,
            CancellationToken cancellationToken)
        {
            var vehicles = await _vehicles.GetByOwnerAsync(request.OwnerId);

            var activeVehicles = vehicles.Where(v => v.IsActive).ToList();
            var inactiveVehicles = vehicles.Where(v => !v.IsActive).ToList();

            var activeDtos = activeVehicles.Select(v => new VehicleDto(
                v.Id,
                v.PlateNumber,
                v.Make,
                v.Model,
                v.Year,
                v.CreatedAt,
                v.IsActive
            )).ToList();

            var inactiveDtos = inactiveVehicles.Select(v => new VehicleDto(
                v.Id,
                v.PlateNumber,
                v.Make,
                v.Model,
                v.Year,
                v.CreatedAt,
                v.IsActive
            )).ToList();

            var result = new MyVehiclesResultDto(
                activeDtos,
                inactiveDtos
            );

            return OperationResult<MyVehiclesResultDto>.Success(result);
        }
    }
}
