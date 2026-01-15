using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Onboarding.Commands
{
    public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, OperationResult<Unit>>
    {
        private readonly IUserRepository _usersRepository;

        public CompleteOnboardingCommandHandler(IUserRepository users)
        {
            _usersRepository = users;
        }
        public async Task<OperationResult<Unit>> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
        {
            var user = await _usersRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return OperationResult<Unit>.Failure("User not found.");
            }

            if (user.OnboardingCompleted)
                return OperationResult<Unit>.Success(Unit.Value);

            user.CompleteOnboarding();
            await _usersRepository.SaveChangesAsync();
            return OperationResult<Unit>.Success(Unit.Value);
        }
    }
}
