using ApplicationLayer.Audit;

namespace Tests.Fakes;

public class FakeAuditLogger : IAuditLogger
{
    public Task LogAsync(
        string action,
        string targetType,
        string targetId,
        object? meta,
        CancellationToken ct = default)
        => Task.CompletedTask;
}
