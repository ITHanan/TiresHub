using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using DomainLayer.Models;
using InfrastructureLayer.Data;
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
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.UserName!.ToLower() == username.ToLower());
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
