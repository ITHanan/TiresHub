using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TireSet.Command.CeateTire
{
    public record CreateTireSetCommand(
        Guid OwnerId,
        Guid VehicleId,
        TireType TireType,
        string Size,
        string Brand,
        string? Notes
    ) : IRequest<OperationResult<Guid>>;
}
