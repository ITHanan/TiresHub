using ApplicationLayer.Features.Bookings.Commands.AssignEmployee;
using ApplicationLayer.Features.Bookings.Events;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.ShopownerTests.BookingTests
{
    public class AssignEmployeeCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Publish_Event_When_Assignment_Succeeds()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var bookingRepoMock = new Mock<IBookingRepository>();
            var userRepoMock = new Mock<IUserRepository>();
            var companyRepoMock = new Mock<ICompanyRepository>();
            var auditRepoMock = new Mock<IAuditRepository>();

            var branchId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var vehicleId = Guid.NewGuid();

            var booking = Booking.Create(
                ServiceType.ChangeTires,
                DateTime.UtcNow.AddDays(1),
                vehicleId,
                branchId,
                TireType.Summer,
                null
            );
            typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

            bookingRepoMock
                .Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var employee = new User("Emp", "emp@test.com", null, UserRole.Employee);
            employee.AssignBranch(branchId);

            userRepoMock
                .Setup(x => x.GetByIdAsync(employee.Id))
                .ReturnsAsync(employee);

            companyRepoMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new ApplicationLayer.Features.Bookings.Commands.AssignEmployee.AssignEmployeeCommandHandler(
                bookingRepoMock.Object,
                userRepoMock.Object,
                companyRepoMock.Object,
                auditRepoMock.Object,
                mediatorMock.Object
            );

            var managerId = Guid.NewGuid();

            var command = new AssignEmployeeCommand(
                ActorUserId: managerId,
                ActorRole: UserRole.ShopManager,
                ActorBranchId: branchId,
                BookingId: bookingId,
                EmployeeId: employee.Id
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            mediatorMock.Verify(x => x.Publish(It.IsAny<EmployeeAssignedToBookingEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
