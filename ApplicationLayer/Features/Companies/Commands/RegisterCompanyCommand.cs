using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;


namespace ApplicationLayer.Features.Companies.Commands
{
    public record RegisterCompanyCommand(string Name) : IRequest<Guid>;
}
