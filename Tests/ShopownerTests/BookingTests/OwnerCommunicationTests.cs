using ApplicationLayer.Features.OwnerCommunication;
using ApplicationLayer.Interfaces;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.Users;
using DomainLayer.Vehicles;
using InfrastructureLayer.Repositories;
using Moq;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.BookingTests;

public class OwnerCommunicationTests
{
    [Fact]
    public async Task Send_includes_evidence_and_persists_sent_log()
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, owner) = await Seed(db);
        var sender = new Mock<IOwnerMessageSender>();
        var service = Service(db, sender.Object, Manager(booking));
        var result = await service.SendAsync(booking.Id, "Please review", "Email", CancellationToken.None);
        Assert.Equal("sent", result.Status);
        Assert.Equal("Email", result.Channel);
        Assert.Single(result.EvidenceRefs);
        sender.Verify(x => x.SendEmailAsync(owner.UserEmail, "Please review", It.Is<IReadOnlyList<string>>(p => p.Count == 1 && p[0] == "https://example.com/tire.jpg"), It.IsAny<CancellationToken>()), Times.Once);
        db.ChangeTracker.Clear();
        var log = Assert.Single(await service.LogsAsync(booking.Id, CancellationToken.None));
        Assert.Equal(result.Id, log.Id);
        Assert.Equal("sent", log.Status);
    }

    [Fact]
    public async Task Failed_delivery_is_persisted_and_returns_log_id()
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, _) = await Seed(db);
        var sender = new Mock<IOwnerMessageSender>();
        sender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());
        var service = Service(db, sender.Object, Manager(booking));
        var error = await Assert.ThrowsAsync<CommunicationException>(() => service.SendAsync(booking.Id, "Please review", "Email", CancellationToken.None));
        Assert.Equal(502, error.StatusCode);
        db.ChangeTracker.Clear();
        var log = Assert.Single(await service.LogsAsync(booking.Id, CancellationToken.None));
        Assert.Equal(error.LogId, log.Id);
        Assert.Equal("send_failed", log.Status);
    }

    [Fact]
    public async Task Missing_email_blocks_sending()
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, owner) = await Seed(db);
        owner.UserEmail = "";
        await db.SaveChangesAsync();
        var sender = new Mock<IOwnerMessageSender>();
        var service = Service(db, sender.Object, Manager(booking));
        Assert.False((await service.PreviewAsync(booking.Id, CancellationToken.None)).CanSend);
        var error = await Assert.ThrowsAsync<CommunicationException>(() => service.SendAsync(booking.Id, "Please review", "Email", CancellationToken.None));
        Assert.Equal(400, error.StatusCode);
        Assert.Empty(db.CommunicationLogs);
        sender.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Xunit.Theory]
    [InlineData("invalid-address")]
    [InlineData("name@example.com\nBcc: other@example.com")]
    public async Task Invalid_owner_email_blocks_preview_and_send(string email)
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, owner) = await Seed(db);
        owner.UserEmail = email;
        await db.SaveChangesAsync();
        var sender = new Mock<IOwnerMessageSender>();
        var service = Service(db, sender.Object, Manager(booking));

        Assert.False((await service.PreviewAsync(booking.Id, CancellationToken.None)).CanSend);
        var error = await Assert.ThrowsAsync<CommunicationException>(() => service.SendAsync(booking.Id, "Please review", "Email", CancellationToken.None));
        Assert.Equal(400, error.StatusCode);
        Assert.Empty(db.CommunicationLogs);
        sender.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Xunit.Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Missing_report_or_photos_blocks_sending(bool removeReport)
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, _) = await Seed(db);
        var report = Assert.Single(db.InspectionReports);
        if (removeReport) db.InspectionReports.Remove(report);
        else db.InspectionPhotos.RemoveRange(report.Photos);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var sender = new Mock<IOwnerMessageSender>();
        var service = Service(db, sender.Object, Manager(booking));
        var preview = await service.PreviewAsync(booking.Id, CancellationToken.None);
        Assert.False(preview.CanSend);
        var error = await Assert.ThrowsAsync<CommunicationException>(() => service.SendAsync(booking.Id, "Please review", "Email", CancellationToken.None));
        Assert.Equal(400, error.StatusCode);
        sender.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Xunit.Theory]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    public async Task Decision_starts_pending_and_owner_can_respond(string response)
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, owner) = await Seed(db);
        var manager = Service(db, Mock.Of<IOwnerMessageSender>(), Manager(booking));
        Assert.Equal("Pending", (await manager.GetDecisionAsync(booking.Id, CancellationToken.None)).Status);
        var service = Service(db, Mock.Of<IOwnerMessageSender>(), new FakeCurrentUser { UserId = owner.Id, Role = UserRole.VehicleOwner });
        var result = await service.RecordDecisionAsync(booking.Id, response, CancellationToken.None);
        Assert.Equal(response, result.Status);
        Assert.Equal("App", result.Channel);
        Assert.NotNull(result.Timestamp);
        db.ChangeTracker.Clear();
        Assert.Equal(response, (await manager.GetDecisionAsync(booking.Id, CancellationToken.None)).Status);
    }

    [Fact]
    public async Task Other_owner_cannot_decide_and_employee_cannot_read_logs()
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, _) = await Seed(db);
        var audit = new Mock<IAuditRepository>();
        var otherOwner = Service(db, Mock.Of<IOwnerMessageSender>(), new FakeCurrentUser { Role = UserRole.VehicleOwner }, audit.Object);
        var error = await Assert.ThrowsAsync<CommunicationException>(() => otherOwner.RecordDecisionAsync(booking.Id, "Approved", CancellationToken.None));
        Assert.Equal(403, error.StatusCode);
        var employee = Service(db, Mock.Of<IOwnerMessageSender>(), new FakeCurrentUser { Role = UserRole.Employee }, audit.Object);
        error = await Assert.ThrowsAsync<CommunicationException>(() => employee.LogsAsync(booking.Id, CancellationToken.None));
        Assert.Equal(403, error.StatusCode);
        audit.Verify(a => a.LogAsync(It.IsAny<Guid?>(), "UnauthorizedOwnerCommunication", "Booking", booking.Id, false, It.IsAny<string>(), null), Times.Exactly(2));
    }

    [Fact]
    public async Task Manager_from_another_branch_is_denied()
    {
        using var db = TestDbFactory.CreateDb();
        var (booking, _) = await Seed(db);
        var service = Service(db, Mock.Of<IOwnerMessageSender>(), new FakeCurrentUser { Role = UserRole.ShopManager, BranchId = Guid.NewGuid() });
        var error = await Assert.ThrowsAsync<CommunicationException>(() => service.PreviewAsync(booking.Id, CancellationToken.None));
        Assert.Equal(403, error.StatusCode);
    }

    private static FakeCurrentUser Manager(Booking b) => new() { Role = UserRole.ShopManager, BranchId = b.BranchId };
    private static OwnerCommunicationService Service(InfrastructureLayer.Persistence.AppDbContext db, IOwnerMessageSender sender, FakeCurrentUser user, IAuditRepository? audit = null)
        => new(new OwnerCommunicationRepository(db), sender, user, audit ?? Mock.Of<IAuditRepository>());

    private static async Task<(Booking, User)> Seed(InfrastructureLayer.Persistence.AppDbContext db)
    {
        var owner = new User("Owner", "owner@example.com", null, UserRole.VehicleOwner);
        var vehicle = new Vehicle("ABC123", owner.Id);
        var booking = Booking.Create(ServiceType.ChangeTires, DateTime.UtcNow.AddDays(1), vehicle.Id, Guid.NewGuid(), TireType.Summer, null);
        var report = new InspectionReport(booking.Id, "Review tires");
        report.AddPhoto(new InspectionPhoto(report.Id, "https://example.com/tire.jpg"));
        db.Users.Add(owner);
        db.Vehicles.Add(vehicle);
        db.Bookings.Add(booking);
        db.InspectionReports.Add(report);
        await db.SaveChangesAsync();
        return (booking, owner);
    }
}
