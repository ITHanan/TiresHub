using DomainLayer.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface ILoginAuditRepository
    {
        Task AddAsync(LoginAuditLog log);

        Task LogAsync(
           Guid? userId,
           string identifier,
           string role,
           bool success,
           string? reason = null);

        Task<List<LoginAuditLog>> GetRecentAsync(int take = 100);
        Task SaveChangesAsync();
    }
}
