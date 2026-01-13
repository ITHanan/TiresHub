using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Authorize.Queries.Login
{
    public class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            RuleFor(user => user.UserEmail).NotEmpty().WithMessage("UserEmail is required.");
            RuleFor(user => user.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
}
