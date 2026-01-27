using ApplicationLayer.Features.TireSet.Command.CeateTire;
using DomainLayer.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.TireSet.Validators
{
    public class CreateTireSetCommandValidator : AbstractValidator<CreateTireSetCommand>
    {
        public CreateTireSetCommandValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEmpty().WithMessage("VehicleId is required.");

            RuleFor(x => x.TireType)
                 .Must(t => Enum.IsDefined(typeof(TireType), t))
                 .WithMessage("Tire type is required.");

            RuleFor(x => x.Size)
                .NotEmpty().WithMessage("Tire size is required.")
                .MaximumLength(50);

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Tire brand is required.")
                .MaximumLength(80);

            RuleFor(x => x.Notes)
                .MaximumLength(500);
        }
    }
}
