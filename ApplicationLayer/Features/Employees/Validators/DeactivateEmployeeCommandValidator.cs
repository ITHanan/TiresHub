using ApplicationLayer.Features.Employees.Commands;
using FluentValidation;

namespace ApplicationLayer.Features.Employees.Validators
{
    public class DeactivateEmployeeCommandValidator : AbstractValidator<DeactivateEmployeeCommand>
    {
        public DeactivateEmployeeCommandValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required.");
        }
    }
}
