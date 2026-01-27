using ApplicationLayer.Features.TireSet.Dtos;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TireSet.Queries
{
    public record GetVehicleTireSetsQuery(Guid OwnerId, Guid VehicleId)
       : IRequest<OperationResult<List<TireSetDto>>>;
}
