using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using InfrastructureLayer.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repositories
{
    public class LoginAuditRepository : ILoginAuditRepository
    {
        private readonly AppDbContext _context;

        public LoginAuditRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LoginAuditLog log)
        {
            await _context.LoginAuditLogs.AddAsync(log);
        }

        public async Task LogAsync(
           Guid? userId,
           string identifier,
           string role,
           bool success,
           string? reason = null)
        {
            var log = new LoginAuditLog(
                userId,
                identifier,
                role,
                success,
                reason);

            await _context.LoginAuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
