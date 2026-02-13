using ApplicationLayer.Features.Employees.Dtos;
using MediatR;

namespace ApplicationLayer.Features.Employees.Commands
{
    public record CreateEmployeeCommand(
        string Name,
        string? Email,
        string? Phone
    ) : IRequest<EmployeeDto>;
}
