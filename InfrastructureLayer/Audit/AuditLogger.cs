using ApplicationLayer.Audit;
using ApplicationLayer.Common.Mappings;
using DomainLayer.Auditing;
using InfrastructureLayer.Persistence;
using Microsoft.VisualStudio.Services.Audit;
using System.Text.Json;

namespace InfrastructureLayer.Audit;

public class AuditLogger : IAuditLogger
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AuditLogger(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task LogAsync(string action, string targetType, string targetId, object? details = null, CancellationToken ct = default)
    {
        var entry = new AuditLog(
         _currentUser.UserId,
         action,
         targetType,
         Guid.Parse(targetId)
     );


        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync(ct);
    }


}

