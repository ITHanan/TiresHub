using ApplicationLayer.Features.Vehicle.Dtos;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Command.CreateVehicle
{
    public record CreateVehicleCommand(
        Guid OwnerId,
        string PlateNumber,
        string? Make,
        string? Model,
        int? Year)
        : IRequest<OperationResult<Guid>>;
   
}
