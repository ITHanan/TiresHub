using ApplicationLayer.Features.Bookings.Events;
using ApplicationLayer.Features.Bookings.Notifications;
using ApplicationLayer.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Notifications;

public class EmployeeAssignedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_Calls_NotificationService_With_Correct_Message()
    {
        // Arrange
        var notificationServiceMock = new Mock<INotificationService>();
        var handler = new EmployeeAssignedNotificationHandler(notificationServiceMock.Object);

        var employeeId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var notification = new EmployeeAssignedToBookingEvent(employeeId, bookingId);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(s => s.NotifyAsync(
            employeeId,
            It.Is<string>(msg => msg.Contains(bookingId.ToString())),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
