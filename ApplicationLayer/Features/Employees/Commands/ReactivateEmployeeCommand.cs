using ApplicationLayer.Features.Employees.Dtos;
using MediatR;

namespace ApplicationLayer.Features.Employees.Commands
{
    public record ReactivateEmployeeCommand(Guid EmployeeId) : IRequest<EmployeeDto>;
}
