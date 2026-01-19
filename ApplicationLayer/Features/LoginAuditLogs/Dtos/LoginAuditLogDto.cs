using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.LoginAuditLogs.Dtos
{
    public record LoginAuditLogDto(
    Guid? UserId,
    string Identifier,
    string Role,
    bool Success,
    string? Reason,
    DateTime CreatedAt
);
}
