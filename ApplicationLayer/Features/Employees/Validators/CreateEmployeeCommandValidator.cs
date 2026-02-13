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

            // Email is currently required (phone-only support not yet implemented)
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required right now (phone-only not supported yet).")
                .EmailAddress().WithMessage("A valid email address is required.");

            // Note: When phone-only support is added, change the above to:
            // RuleFor(x => x)
            //     .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Phone))
            //     .WithMessage("Email or phone is required.");
            // RuleFor(x => x.Email)
            //     .EmailAddress().WithMessage("A valid email address is required.")
            //     .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
