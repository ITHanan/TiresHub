using ApplicationLayer.Features.StaffAuth.Commands.StaffVerifyCode;
using ApplicationLayer.Features.StaffAuth.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;

public class StaffVerifyCodeCommandHandler
    : IRequestHandler<StaffVerifyCodeCommand, OperationResult<StaffAuthResponseDto>>
{
    private readonly IUserRepository _users;
    private readonly IVerificationCodeRepository _codes;
    private readonly IJwtGenerator _jwt;
    private readonly ILoginAuditRepository _audit;

    public StaffVerifyCodeCommandHandler(
        IUserRepository users,
        IVerificationCodeRepository codes,
        IJwtGenerator jwt,
        ILoginAuditRepository audit)
    {
        _users = users;
        _codes = codes;
        _jwt = jwt;
        _audit = audit;
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
        {
            await _audit.LogAsync(
                null,
                request.Identifier,
                "Unknown",
                false,
                "Invalid or expired verification code");

            return OperationResult<StaffAuthResponseDto>.Failure(
                "Invalid or expired verification code.");
        }

        verification.MarkAsUsed();
        await _codes.SaveChangesAsync();

        // 2️⃣ Load user
        var user = await _users.GetByIdentifierAsync(request.Identifier);

        if (user == null)
        {
            await _audit.LogAsync(
                null,
                request.Identifier,
                "Unknown",
                false,
                "Account not found");

            return OperationResult<StaffAuthResponseDto>.Failure(
                "Account not found.");
        }

        // 3️⃣ Validate staff role
        if (user.Role is not (UserRole.ShopManager or UserRole.Employee))
        {
            await _audit.LogAsync(
                user.Id,
                request.Identifier,
                user.Role.ToString(),
                false,
                "Not a staff role");

            return OperationResult<StaffAuthResponseDto>.Failure(
                "This account is not allowed to use staff login.");
        }

        // 4️⃣ Validate active
        if (!user.IsActive)
        {
            await _audit.LogAsync(
                user.Id,
                request.Identifier,
                user.Role.ToString(),
                false,
                "Account deactivated");

            return OperationResult<StaffAuthResponseDto>.Failure(
                "Your account has been deactivated.");
        }

        // 5️⃣ Branch scope
        if (user.BranchId == null)
        {
            await _audit.LogAsync(
                user.Id,
                request.Identifier,
                user.Role.ToString(),
                false,
                "No branch assigned");

            return OperationResult<StaffAuthResponseDto>.Failure(
                "This staff account is not assigned to any branch. Please contact your shop owner.");
        }

        // 6️⃣ Success
        var token = _jwt.GenerateToken(user);

        await _audit.LogAsync(
            user.Id,
            request.Identifier,
            user.Role.ToString(),
            true);

        return OperationResult<StaffAuthResponseDto>.Success(
            new StaffAuthResponseDto(
                Token: token,
                Role: user.Role.ToString(),
                BranchId: user.BranchId.Value
            ));
    }
}
