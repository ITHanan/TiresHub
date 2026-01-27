using ApplicationLayer.Features.Vehicles.Dtos;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Queries
{
    public record GetMyVehiclesQuery(Guid OwnerId)
        : IRequest<OperationResult<List<VehicleDto>>>;
}
