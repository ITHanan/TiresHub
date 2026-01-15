using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Onboarding.Commands
{
    public record CompleteOnboardingCommand(Guid UserId): IRequest<OperationResult<Unit>>;
    
}
