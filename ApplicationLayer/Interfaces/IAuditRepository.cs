namespace ApplicationLayer.Interfaces
{
    public interface IAuditRepository
    {
        Task LogAsync(
        Guid? userId,
        string action,
        string entityType,
        Guid? entityId,
        bool success,
        string? reason = null,
        string? metadata = null);
    }
}
