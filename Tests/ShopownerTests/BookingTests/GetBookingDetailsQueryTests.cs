using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.ShopownerTests.BookingTests
{
    public class GetBookingDetailsQueryTests
    {
        private readonly Mock<ICurrentUser> _currentUser = new();
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IAuditRepository> _auditRepo = new();

        private GetBookingDetailsQueryHandler CreateHandler()
            => new(
                _currentUser.Object,
                _bookingRepo.Object,
                _userRepo.Object,
                _auditRepo.Object);

        private void SetupAuthenticatedUser(UserRole role, Guid userId)
        {
            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(role);
            _currentUser.Setup(x => x.UserId).Returns(userId);
        }

        private static DomainLayer.Users.User CreateShopManager(Guid? branchId = null)
        {
            var user = new DomainLayer.Users.User(
                "Manager",
                "manager@test.com",
                null,
                UserRole.ShopManager);

            if (branchId.HasValue)
            {
                user.AssignBranch(branchId.Value);
            }

            return user;
        }

        private static BookingDetailsDto CreateBooking(
            Guid bookingId,
            Guid branchId,
            Guid? assignedEmployeeId = null)
            => new()
            {
                Id = bookingId,
                BranchId = branchId,
                AssignedEmployeeId = assignedEmployeeId,
                VehiclePlateNumber = "ABC123",
                ServiceType = ServiceType.ChangeTires,
                Status = BookingStatus.Confirmed,
                BranchName = "Branch 1"
            };

        [Fact]
        public async Task Handle_ShouldReturnBookingDetails_WhenShopManagerIsAuthorized()
        {
            var userId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            var booking = CreateBooking(bookingId, branchId);

            SetupAuthenticatedUser(UserRole.ShopManager, userId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(CreateShopManager(branchId));

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(bookingId, cancellationToken))
                .ReturnsAsync(booking);

            var result = await CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                cancellationToken);

            result.Should().BeSameAs(booking);
            result.Id.Should().Be(bookingId);
            result.VehiclePlateNumber.Should().Be("ABC123");

            _bookingRepo.Verify(
                x => x.GetBookingDetailsAsync(bookingId, cancellationToken),
                Times.Once);

            _auditRepo.Verify(x => x.LogAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnBookingDetails_WhenEmployeeIsAssigned()
        {
            var employeeId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var booking = CreateBooking(bookingId, Guid.NewGuid(), employeeId);

            SetupAuthenticatedUser(UserRole.Employee, employeeId);

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var result = await CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            result.Should().BeSameAs(booking);
            result.Id.Should().Be(bookingId);

            _auditRepo.Verify(x => x.LogAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserIsNotAuthenticated()
        {
            _currentUser.Setup(x => x.IsAuthenticated).Returns(false);

            Func<Task> act = () => CreateHandler().Handle(
                new GetBookingDetailsQuery(Guid.NewGuid()),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*not authenticated*");

            _bookingRepo.Verify(x => x.GetBookingDetailsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserHasUnsupportedRole()
        {
            var bookingId = Guid.NewGuid();

            SetupAuthenticatedUser(UserRole.VehicleOwner, Guid.NewGuid());

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateBooking(bookingId, Guid.NewGuid()));

            Func<Task> act = () => CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Only shop managers or the assigned employee*");
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenShopManagerIsNotFound()
        {
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            SetupAuthenticatedUser(UserRole.ShopManager, userId);

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateBooking(bookingId, Guid.NewGuid()));

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((DomainLayer.Users.User?)null);

            Func<Task> act = () => CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*User not found*");
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenShopManagerHasNoBranch()
        {
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            SetupAuthenticatedUser(UserRole.ShopManager, userId);

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateBooking(bookingId, Guid.NewGuid()));

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(CreateShopManager());

            Func<Task> act = () => CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Shop manager is not assigned to any branch.*");
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenBookingIsNotFound()
        {
            var bookingId = Guid.NewGuid();

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BookingDetailsDto?)null);

            Func<Task> act = () => CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Booking not found*");
        }

        [Fact]
        public async Task Handle_ShouldThrowAndLog_WhenBookingBelongsToAnotherBranch()
        {
            var userId = Guid.NewGuid();
            var userBranchId = Guid.NewGuid();
            var bookingBranchId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            SetupAuthenticatedUser(UserRole.ShopManager, userId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(CreateShopManager(userBranchId));

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateBooking(bookingId, bookingBranchId));

            Func<Task> act = () => CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*do not have access*");

            _auditRepo.Verify(x => x.LogAsync(
                userId,
                "UnauthorizedBookingAccess",
                "Booking",
                bookingId,
                false,
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowAndLog_WhenEmployeeIsNotAssignedToBooking()
        {
            var employeeId = Guid.NewGuid();
            var assignedEmployeeId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            SetupAuthenticatedUser(UserRole.Employee, employeeId);

            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateBooking(
                    bookingId,
                    Guid.NewGuid(),
                    assignedEmployeeId));

            Func<Task> act = () => CreateHandler().Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*do not have access*");

            _auditRepo.Verify(x => x.LogAsync(
                employeeId,
                "UnauthorizedBookingAccessByEmployee",
                "Booking",
                bookingId,
                false,
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }
    }
}
