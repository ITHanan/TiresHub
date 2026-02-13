using DomainLayer.Common;

namespace DomainLayer.Notifications;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Message { get; private set; } = default!;
    public bool IsRead { get; private set; }

    protected Notification() { }

    public Notification(Guid userId, string message)
    {
        UserId = userId;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        IsRead = false;
    }

    public void MarkAsRead() => IsRead = true;
}
