using InfrastructureLayer.Services;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;
using DomainLayer.Notifications;
using System.Threading;
using System;
using FluentAssertions;

namespace Tests.Notifications
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task NotifyAsync_Should_Save_Notification()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "NotificationTestDb")
                .Options;

            using var context = new AppDbContext(options);

            var service = new NotificationService(context);

            var userId = Guid.NewGuid();

            await service.NotifyAsync(userId, "Test message", CancellationToken.None);

            var savedNotification = context.Notifications.FirstOrDefault();

            savedNotification.Should().NotBeNull();
            savedNotification!.UserId.Should().Be(userId);
            savedNotification.Message.Should().Be("Test message");
        }
    }
}
