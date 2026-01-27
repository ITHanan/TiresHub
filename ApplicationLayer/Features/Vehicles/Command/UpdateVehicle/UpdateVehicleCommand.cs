using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Command.UpdateVehicle
{
    public record UpdateVehicleCommand(
      Guid OwnerId,
      Guid VehicleId,
      string? Make,
      string? Model,
      int? Year
  ) : IRequest<OperationResult<bool>>;
}
