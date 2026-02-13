namespace ApplicationLayer.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(Guid userId, string message, CancellationToken cancellationToken);
}
