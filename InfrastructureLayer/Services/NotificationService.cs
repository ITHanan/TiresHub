using ApplicationLayer.Interfaces;
using InfrastructureLayer.Persistence;
using DomainLayer.Notifications;

namespace InfrastructureLayer.Services;

public class NotificationService : ApplicationLayer.Interfaces.INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task NotifyAsync(Guid userId, string message, CancellationToken cancellationToken)
    {
        var notification = new Notification(userId, message);
        await _context.Set<Notification>().AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
