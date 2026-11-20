using ApplicationLayer.Features.Bookings.Queries.GetInspectionReportByBooking;
using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.Users;
using DomainLayer.Vehicles;
using InfrastructureLayer.Middleware;
using InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.BookingTests;

public class InspectionReportTests
{
    [Fact]
    public async Task Manager_reads_report_and_booking_context()
    {
        using var db = TestDbFactory.CreateDb();
        var customer = new User("Customer Name", "customer@example.com", null, UserRole.VehicleOwner);
        var employee = new User("Employee Name", "employee@example.com", null, UserRole.Employee);
        var vehicle = new Vehicle("ABC123", customer.Id);
        var branchId = Guid.NewGuid();
        var booking = Booking.Create(ServiceType.ChangeTires, DateTime.UtcNow.AddDays(1), vehicle.Id, branchId, TireType.Summer, null);
        var report = new InspectionReport(booking.Id, "Tread worn", employee.Id);
        report.AddPhoto(new InspectionPhoto(report.Id, "https://example.com/photo.jpg"));
        db.Users.AddRange(customer, employee);
        db.Vehicles.Add(vehicle);
        db.Bookings.Add(booking);
        db.InspectionReports.Add(report);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new GetInspectionReportByBookingQueryHandler(new BookingRepository(db),
            new FakeCurrentUser { Role = UserRole.ShopManager, BranchId = branchId }, Mock.Of<IAuditRepository>());
        var result = await handler.Handle(new(booking.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(booking.Id, result.Data!.Booking.BookingId);
        Assert.Equal(booking.AppointmentDate, result.Data.Booking.AppointmentDate);
        Assert.Equal(ServiceType.ChangeTires, result.Data.Booking.ServiceType);
        Assert.Equal("Customer Name", result.Data.Booking.CustomerName);
        Assert.Equal("Tread worn", result.Data.Report!.Notes);
        Assert.Equal("Employee Name", result.Data.Report.CreatedByName);
        Assert.Equal(report.CreatedAt, result.Data.Report.CreatedAt);
        Assert.Equal(new[] { "https://example.com/photo.jpg" }, result.Data.Report.Photos);
    }

    [Fact]
    public async Task Missing_report_returns_null_but_missing_booking_is_an_error()
    {
        using var db = TestDbFactory.CreateDb();
        var booking = Booking.Create(ServiceType.ChangeTires, DateTime.UtcNow.AddDays(1), Guid.NewGuid(), Guid.NewGuid(), TireType.Summer, null);
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        var handler = new GetInspectionReportByBookingQueryHandler(new BookingRepository(db), new FakeCurrentUser(), Mock.Of<IAuditRepository>());

        var existing = await handler.Handle(new(booking.Id), CancellationToken.None);
        var missing = await handler.Handle(new(Guid.NewGuid()), CancellationToken.None);

        Assert.True(existing.IsSuccess);
        Assert.Null(existing.Data!.Report);
        Assert.False(missing.IsSuccess);
        Assert.Equal("Booking not found.", missing.ErrorMessage);
    }

    [Xunit.Theory]
    [InlineData(UserRole.Employee)]
    [InlineData(UserRole.VehicleOwner)]
    [InlineData(UserRole.ShopManager)]
    public async Task Unauthorized_role_or_branch_is_denied_and_audited(UserRole role)
    {
        using var db = TestDbFactory.CreateDb();
        var booking = Booking.Create(ServiceType.ChangeTires, DateTime.UtcNow.AddDays(1), Guid.NewGuid(), Guid.NewGuid(), TireType.Summer, null);
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        var user = new FakeCurrentUser { Role = role, BranchId = Guid.NewGuid() };
        var audit = new Mock<IAuditRepository>();
        var handler = new GetInspectionReportByBookingQueryHandler(new BookingRepository(db), user, audit.Object);

        var result = await handler.Handle(new(booking.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("access", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        audit.Verify(a => a.LogAsync(user.UserId, AuditActions.UnauthorizedInspectionReportAccess, "Booking", booking.Id,
            false, It.IsAny<string>(), null), Times.Once);
    }

    [Xunit.Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task Report_modifications_are_blocked_and_audited(string method)
    {
        var bookingId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = $"/api/bookings/{bookingId}/inspection-report/photos";
        context.Response.Body = new MemoryStream();
        var nextCalled = false;
        var middleware = new InspectionReportReadOnlyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var audit = new Mock<IAuditRepository>();

        await middleware.InvokeAsync(context, audit.Object);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status405MethodNotAllowed, context.Response.StatusCode);
        audit.Verify(a => a.LogAsync(null, AuditActions.InspectionReportModificationAttempt, "Booking", bookingId,
            false, "Inspection reports are read-only.", method), Times.Once);
    }
}
