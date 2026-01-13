using ApplicationLayer.Interfaces;
using AutoMapper;
using DomainLayer.Common;
using DomainLayer.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Authorize.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, OperationResult<string>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(IAuthRepository authRepository, IJwtGenerator jwtGenerator, IMapper mapper)
        {
            _authRepository = authRepository;
            _jwtGenerator = jwtGenerator;
            _mapper = mapper;
        }

        public async Task<OperationResult<string>> Handle(
     RegisterCommand request,
     CancellationToken cancellationToken)
        {
            try
            {
                // 1️⃣ Normalize email ONCE
                var normalizedEmail = request.UserEmail.Trim().ToLower();

                // 2️⃣ Check for existing email
                if (await _authRepository.EmailExistsAsync(normalizedEmail))
                {
                    return OperationResult<string>.Failure(
                        "Email is already registered.");
                }

                // 3️⃣ Map request → User
                var user = _mapper.Map<User>(request);

                // 4️⃣ Apply normalized email explicitly
                user.UserEmail = normalizedEmail;

                // 5️⃣ Hash password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 6️⃣ Persist user
                await _authRepository.CreateUserAsync(user);
                await _authRepository.SaveChangesAsync();

                // 7️⃣ Generate JWT
                var token = _jwtGenerator.GenerateToken(user);

                return OperationResult<string>.Success(token);
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Failure(
                    $"Error during registration: {ex.Message}");
            }
        }

    }
}
