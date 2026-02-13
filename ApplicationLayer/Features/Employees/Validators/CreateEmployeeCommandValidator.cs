using ApplicationLayer.Features.Employees.Commands;
using FluentValidation;

namespace ApplicationLayer.Features.Employees.Validators
{
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            // At least one contact method must be provided
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Email or phone is required.");

            // Email format validation when provided
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email address is required.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            // For now, require email (temporary business rule)
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required right now (phone-only not supported yet).");
        }
    }
}
