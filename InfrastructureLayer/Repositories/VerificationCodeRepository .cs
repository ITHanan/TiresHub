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
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        private readonly AppDbContext _context;

        public VerificationCodeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(VerificationCode code)
        {
            await _context.VerificationCodes.AddAsync(code);
        }

        public async Task<VerificationCode?> GetValidCodeAsync(
            string identifier,
            string code)
        {
            var now = DateTime.UtcNow;

            return await _context.VerificationCodes
                .Where(vc =>
                    vc.Identifier == identifier &&
                    vc.Code == code &&
                    !vc.Used &&
                    vc.ExpiresAt >= now)
                .OrderByDescending(vc => vc.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidateAsync(VerificationCode code)
        {
            code.MarkAsUsed();
            _context.VerificationCodes.Update(code);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
