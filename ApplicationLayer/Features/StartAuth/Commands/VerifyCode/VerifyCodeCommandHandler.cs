using ApplicationLayer.Features.StartAuth.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Enums;
using DomainLayer.Users;
using MediatR;

namespace ApplicationLayer.Features.StartAuth.Commands.VerifyCode
{
    public class VerifyCodeCommandHandler
    : IRequestHandler<VerifyCodeCommand, OperationResult<AuthResponseDto>>
    {
        private readonly IUserRepository _users;
        private readonly IVerificationCodeRepository _codes;
        private readonly IJwtGenerator _jwt;

        public VerifyCodeCommandHandler(
        IUserRepository users,
        IVerificationCodeRepository codes,
        IJwtGenerator jwt)
        {
            _users = users;
            _codes = codes;
            _jwt = jwt;
        }
        public async Task<OperationResult<AuthResponseDto>> Handle(VerifyCodeCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate verification code
            var verification = await _codes.GetValidCodeAsync(
                request.Identifier,
                request.Code);

            if (verification == null)
            {
                return OperationResult<AuthResponseDto>.Failure(
                    "Invalid or expired verification code.");
            }

            // 2. Mark code as used
            verification.MarkAsUsed();


            // 3. Load or create user
            var user = await _users.GetByIdentifierAsync(request.Identifier);
            var isFirstLogin = false;

            if (user == null)
            {
                user = new User(
                    name: "New User",
                    email: request.Identifier,
                    phone: "",
                    role: UserRole.VehicleOwner // role already validated in StartAuth
                );

                await _users.AddAsync(user);
                isFirstLogin = true;
            }
            // 4. Persist changes
            await _codes.SaveChangesAsync();
            await _users.SaveChangesAsync();

            // 5. Generate JWT
            var token = _jwt.GenerateToken(user);

            // 6. Return auth response
            return OperationResult<AuthResponseDto>.Success(
                   new AuthResponseDto(
                       token: token,
                       isFirstLogin: isFirstLogin,
                       role: user.Role.ToString()
                   )
            );

        }
    }
}
