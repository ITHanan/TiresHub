using ApplicationLayer.Features.Employees.Dtos;
using MediatR;

namespace ApplicationLayer.Features.Employees.Queries
{
    public record GetBranchEmployeesQuery : IRequest<List<EmployeeDto>>;
}
