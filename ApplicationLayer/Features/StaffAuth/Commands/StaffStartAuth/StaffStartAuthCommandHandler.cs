using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Enums;
using DomainLayer.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StaffAuth.Commands.StaffStartAuth
{
    public class StaffStartAuthCommandHandler : IRequestHandler<StaffStartAuthCommand, OperationResult<Unit>>
    {
        private readonly IUserRepository _users;
        private readonly IVerificationCodeRepository _codes;

        public StaffStartAuthCommandHandler(
            IUserRepository users,
            IVerificationCodeRepository codes)
        {
            _users = users;
            _codes = codes;
        }

        public async Task<OperationResult<Unit>> Handle(
            StaffStartAuthCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Find user by email or phone
            var user = await _users.GetByIdentifierAsync(request.Identifier);

            if (user == null)
                return OperationResult<Unit>.Failure(
                    "This account does not exist. Please contact your administrator.");

            // 2️⃣ Ensure role is staff
            if (user.Role is not (UserRole.ShopManager or UserRole.Employee))
                return OperationResult<Unit>.Failure(
                    "This account is not allowed to use staff login.");

            // 3️⃣ Ensure account is active
            if (!user.IsActive)
                return OperationResult<Unit>.Failure(
                    "Your account has been deactivated.");

            // 4️⃣ Generate mocked verification code
            var code = Random.Shared.Next(100000, 999999).ToString();

            var verification = new VerificationCode(
                identifier: request.Identifier,
                code: code,
                role: user.Role
            );

            await _codes.AddAsync(verification);
            await _codes.SaveChangesAsync();

            // 5️⃣ Mock send
          //  Console.WriteLine($"[STAFF LOGIN] Code for {request.Identifier}: {code}");

            return OperationResult<Unit>.Success(Unit.Value);
        }
       
    }
}
