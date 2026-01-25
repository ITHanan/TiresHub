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
    public record UpdateTireSetCommand(
       Guid ActorUserId,
       UserRole ActorRole,
       Guid TireSetId,
       string Size,
       string Brand,
       string? Notes
   ) : IRequest<OperationResult<bool>>;
}
