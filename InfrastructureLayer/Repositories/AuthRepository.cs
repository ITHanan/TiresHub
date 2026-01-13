using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Users;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        // Add a new user to the database (save is called separately)
        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        // Check if email is already registered
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(user => user.UserEmail!.ToLower() == email.ToLower());
        }

        // Get user by username (used in login)
      

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();

            return await _context.Users
                .FirstOrDefaultAsync(user => user.UserEmail.ToLower() == normalizedEmail);
        }
        // Save changes to database
        public async Task<OperationResult<bool>> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return OperationResult<bool>.Failure($"Saving changes failed: {ex.Message}");
            }
        }
    }
}
