using ApplicationLayer.Features.StaffAuth.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.StaffAuth.Commands.StaffVerifyCode
{
    public class StaffVerifyCodeCommandHandler
        : IRequestHandler<StaffVerifyCodeCommand, OperationResult<StaffAuthResponseDto>>
    {
        private readonly IUserRepository _users;
        private readonly IVerificationCodeRepository _codes;
        private readonly IJwtGenerator _jwt;

        public StaffVerifyCodeCommandHandler(
            IUserRepository users,
            IVerificationCodeRepository codes,
            IJwtGenerator jwt)
        {
            _users = users;
            _codes = codes;
            _jwt = jwt;
        }

        public async Task<OperationResult<StaffAuthResponseDto>> Handle(
            StaffVerifyCodeCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Validate verification code
            var verification = await _codes.GetValidCodeAsync(
                request.Identifier,
                request.Code);

            if (verification == null)
                return OperationResult<StaffAuthResponseDto>.Failure(
                    "Invalid or expired verification code.");

            verification.MarkAsUsed();
            await _codes.SaveChangesAsync();

            // 2️⃣ Load user
            var user = await _users.GetByIdentifierAsync(request.Identifier);

            if (user == null)
                return OperationResult<StaffAuthResponseDto>.Failure(
                    "Account not found.");

            // 3️⃣ Validate staff role
            if (user.Role is not (UserRole.ShopManager or UserRole.Employee))
                return OperationResult<StaffAuthResponseDto>.Failure(
                    "This account is not allowed to use staff login.");

            // 4️⃣ Validate active
            if (!user.IsActive)
                return OperationResult<StaffAuthResponseDto>.Failure(
                    "Your account has been deactivated.");

            // 5️⃣ Generate JWT
            var token = _jwt.GenerateToken(user);

            // ⚠️ IMPORTANT:
            // Assumption: User has exactly ONE BranchId assigned
            var branchId = user.BranchId
                ?? throw new InvalidOperationException("Staff user has no branch assigned.");

            return OperationResult<StaffAuthResponseDto>.Success(
                new StaffAuthResponseDto(
                    Token: token,
                    Role: user.Role.ToString(),
                    BranchId: branchId
                ));
        }
    }
}
