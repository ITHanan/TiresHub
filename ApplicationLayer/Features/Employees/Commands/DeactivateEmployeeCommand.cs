using ApplicationLayer.Features.Employees.Dtos;
using MediatR;

namespace ApplicationLayer.Features.Employees.Commands
{
    public record DeactivateEmployeeCommand(Guid EmployeeId) : IRequest<EmployeeDto>;
}
