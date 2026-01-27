using ApplicationLayer.Features.Vehicles.Command.CreateVehicle;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Vehicles.Validators
{
    public class CreateVehicleCommandValidator: AbstractValidator<CreateVehicleCommand>
    {
        public CreateVehicleCommandValidator()
        {
            RuleFor(x => x.PlateNumber)
           .NotEmpty().WithMessage("License plate is required.")
           .MaximumLength(20);

            RuleFor(x => x.Year)
                .GreaterThan(1900)
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 1)
                .When(x => x.Year.HasValue)
                .WithMessage("Invalid vehicle year.");
        }
    }
}
