using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Bookings.Commands.AssignEmployee
{
    public record AssignEmployeeCommand(
        Guid ActorUserId,
        UserRole ActorRole,
        Guid? ActorBranchId,
        Guid BookingId,
        Guid EmployeeId
    ) : IRequest<OperationResult<Unit>>;

}
