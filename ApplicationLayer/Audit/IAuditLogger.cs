using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Audit;

public interface IAuditLogger
{
    Task LogAsync(
        string Action,
        string TargetType,
        string TargetId,
        object? Details = null,
        CancellationToken ct = default
    );
}

