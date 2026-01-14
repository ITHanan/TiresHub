using ApplicationLayer.Features.StartAuth.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Enums;
using DomainLayer.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StartAuth.Commands
{

    public class StartAuthCommandHandler
    : IRequestHandler<StartAuthCommand, OperationResult<Unit>>
    {
        private readonly IVerificationCodeRepository _codes;

        public StartAuthCommandHandler(IVerificationCodeRepository codes)
        {
            _codes = codes;
        }
        public async Task<OperationResult<Unit>> Handle(StartAuthCommand request, CancellationToken cancellationToken)
        {
            // Block unsupported roles
            if (request.Role is UserRole.ShopManager or UserRole.Employee)
            {
                return OperationResult<Unit>.Failure(
                    "This role cannot be registered directly.");
            }

            // Generate mocked verification code
            var code = Random.Shared.Next(100000, 999999).ToString();

            var verification = new VerificationCode(
                identifier: request.Identifier,
                code: code
            );

            await _codes.AddAsync(verification);
            await _codes.SaveChangesAsync();

            // Mock send (log only)
            Console.WriteLine($"Verification code for {request.Identifier}: {code}");

            return OperationResult<Unit>.Success(Unit.Value);


        }
    }
}
