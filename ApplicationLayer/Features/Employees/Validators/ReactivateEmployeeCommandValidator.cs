using ApplicationLayer.Features.Employees.Commands;
using FluentValidation;

namespace ApplicationLayer.Features.Employees.Validators
{
    public class ReactivateEmployeeCommandValidator : AbstractValidator<ReactivateEmployeeCommand>
    {
        public ReactivateEmployeeCommandValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required.");
        }
    }
}
