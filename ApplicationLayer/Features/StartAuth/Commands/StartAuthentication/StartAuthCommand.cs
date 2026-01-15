using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StartAuth.Commands
{
    public record StartAuthCommand(string Identifier, UserRole Role)
     : IRequest<OperationResult<Unit>>;
}
