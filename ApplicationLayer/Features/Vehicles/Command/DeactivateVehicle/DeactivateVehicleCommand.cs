using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Command.DeactivateVehicle
{
    public record DeactivateVehicleCommand(Guid OwnerId, Guid VehicleId)
       : IRequest<OperationResult<bool>>;
}
