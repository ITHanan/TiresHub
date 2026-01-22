using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using InfrastructureLayer.Persistence;

namespace InfrastructureLayer.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AppDbContext _context;

        public AuditRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
             Guid? userId,
             string action,
             string entityType,
             Guid? entityId,
             bool success,
             string? reason = null,
             string? metadata = null)
        {
            var log = new AuditLog(
                userId,
                action,
                entityType,
                entityId,
                success,
                reason,
                metadata
            );

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
