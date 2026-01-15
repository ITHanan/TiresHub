using ApplicationLayer.Interfaces;
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
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var normalizedEmail = email.ToLower();
            return await _context.Users.AnyAsync(u => u.UserEmail == normalizedEmail);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.ToLower();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserEmail == normalizedEmail);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            var normalized = identifier.Trim().ToLower();

            return await _context.Users.FirstOrDefaultAsync(u =>
                u.UserEmail.ToLower() == normalized ||
                (u.Phone != null && u.Phone == normalized)
            );
        }


        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
