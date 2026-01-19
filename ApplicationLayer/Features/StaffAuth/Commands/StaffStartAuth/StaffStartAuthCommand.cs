using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StaffAuth.Commands.StaffStartAuth
{
    public record StaffStartAuthCommand(string Identifier):IRequest<OperationResult<Unit>>;
   
}
