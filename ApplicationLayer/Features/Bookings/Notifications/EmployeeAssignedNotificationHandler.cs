using ApplicationLayer.Features.Bookings.Events;
using ApplicationLayer.Interfaces;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Notifications;

public class EmployeeAssignedNotificationHandler : INotificationHandler<EmployeeAssignedToBookingEvent>
{
    private readonly INotificationService _notificationService;

    public EmployeeAssignedNotificationHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(EmployeeAssignedToBookingEvent notification, CancellationToken cancellationToken)
    {
        var message = $"You have been assigned to booking {notification.BookingId}";
        await _notificationService.NotifyAsync(notification.EmployeeId, message, cancellationToken);
    }
}
