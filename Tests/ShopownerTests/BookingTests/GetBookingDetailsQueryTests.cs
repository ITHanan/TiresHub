using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using FluentAssertions;
using InfrastructureLayer.Repositories;
using Moq;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

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


        private static DomainLayer.Users.User CreateShopManager(Guid branchId)
        {
            var user = new DomainLayer.Users.User(
                "Manager",
                "manager@test.com",
                null,
                UserRole.ShopManager);

            user.AssignBranch(branchId);
            return user;
        }

        [Fact]
        public async Task HandleShouldReturnBookingDetailsWhenUserIsAuthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(UserRole.ShopManager);
            _currentUser.Setup(x => x.UserId).Returns(userId);

            var user = new DomainLayer.Users.User(
                "Manager",
                "manager@test.com",
                null,
                UserRole.ShopManager);

            user.AssignBranch(branchId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var bookingDto = new BookingDetailsDto
            {
                Id = bookingId,
                BranchId = branchId,
                VehiclePlateNumber = "ABC123",
                ServiceType = ServiceType.ChangeTires,
                Status = BookingStatus.Confirmed,
                BranchName = "Branch 1"
            };
            _bookingRepo
                .Setup(x => x.GetBookingDetailsAsync(
                    bookingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookingDto);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(bookingId);
            result.VehiclePlateNumber.Should().Be("ABC123");

            _auditRepo.Verify(
                x => x.LogAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        // ----------------------------
        // ✅ SUCCESS
        // ----------------------------

        [Fact]
        public async Task Handle_ShouldReturnBookingDetails_WhenUserIsAuthorized()
        {
            var userId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(UserRole.ShopManager);
            _currentUser.Setup(x => x.UserId).Returns(userId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(CreateShopManager(branchId));

            var booking = new BookingDetailsDto
            {
                Id = bookingId,
                BranchId = branchId,
                VehiclePlateNumber = "ABC123",
                ServiceType = ServiceType.ChangeTires,
                Status = BookingStatus.Confirmed,
                BranchName = "Branch 1"
            };

            _bookingRepo.Setup(x =>
                    x.GetBookingDetailsAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new GetBookingDetailsQuery(bookingId),
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(bookingId);
            result.VehiclePlateNumber.Should().Be("ABC123");

            _auditRepo.Verify(
                x => x.LogAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        // ----------------------------
        // ❌ NOT AUTHENTICATED
        // ----------------------------

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserIsNotAuthenticated()
        {
            _currentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = CreateHandler();

            Func<Task> act = () =>
                handler.Handle(
                    new GetBookingDetailsQuery(Guid.NewGuid()),
                    CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*not authenticated*");
        }

        // ----------------------------
        // ❌ NOT SHOP MANAGER
        // ----------------------------

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserIsNotShopManager()
        {
            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(UserRole.VehicleOwner);

            var handler = CreateHandler();

            Func<Task> act = () =>
                handler.Handle(
                    new GetBookingDetailsQuery(Guid.NewGuid()),
                    CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*shop managers*");
        }

        // ----------------------------
        // ❌ USER NOT FOUND
        // ----------------------------

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();

            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(UserRole.ShopManager);
            _currentUser.Setup(x => x.UserId).Returns(userId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((DomainLayer.Users.User?)null);

            var handler = CreateHandler();

            Func<Task> act = () =>
                handler.Handle(
                    new GetBookingDetailsQuery(Guid.NewGuid()),
                    CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*User not found*");
        }

        // ----------------------------
        // ❌ NO BRANCH ASSIGNED
        // ----------------------------

        [Fact]
        public async Task Handle_ShouldThrow_WhenUserHasNoBranch()
        {
            var userId = Guid.NewGuid();

            var user = new DomainLayer.Users.User(
                "Manager",
                "manager@test.com",
                null,
                UserRole.ShopManager);

            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(UserRole.ShopManager);
            _currentUser.Setup(x => x.UserId).Returns(userId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var handler = CreateHandler();

            Func<Task> act = () =>
                handler.Handle(
                    new GetBookingDetailsQuery(Guid.NewGuid()),
                    CancellationToken.None);

                   await act.Should().ThrowAsync<UnauthorizedAccessException>()
                      .Where(e => e.Message.Contains("Shop manager is not assigned to any branch."));
        }

        // ----------------------------
        // ❌ BOOKING NOT FOUND
        // ----------------------------

        [Fact]
        public async Task Handle_ShouldThrow_WhenBookingNotFound()
        {
            var userId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(UserRole.ShopManager);
            _currentUser.Setup(x => x.UserId).Returns(userId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(CreateShopManager(branchId));

            _bookingRepo.Setup(x =>
                    x.GetBookingDetailsAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BookingDetailsDto?)null);

            var handler = CreateHandler();

            Func<Task> act = () =>
                handler.Handle(
                    new GetBookingDetailsQuery(bookingId),
                    CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Booking not found*");
        }

        // ----------------------------
        // ❌ BOOKING FROM DIFFERENT BRANCH (AUDIT)
        // ----------------------------

        [Fact]
        public async Task Handle_ShouldThrowAndLog_WhenBookingFromDifferentBranch()
        {
            var userId = Guid.NewGuid();
            var userBranchId = Guid.NewGuid();
            var bookingBranchId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            _currentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUser.Setup(x => x.Role).Returns(UserRole.ShopManager);
            _currentUser.Setup(x => x.UserId).Returns(userId);

            _userRepo.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(CreateShopManager(userBranchId));

            _bookingRepo.Setup(x =>
                    x.GetBookingDetailsAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BookingDetailsDto
                {
                    Id = bookingId,
                    BranchId = bookingBranchId
                });

            var handler = CreateHandler();

            Func<Task> act = () =>
                handler.Handle(
                    new GetBookingDetailsQuery(bookingId),
                    CancellationToken.None);

                   await act.Should().ThrowAsync<UnauthorizedAccessException>()
                  .WithMessage("*do not have access*");

            _auditRepo.Verify(x =>
                x.LogAsync(
                    userId,
                    "UnauthorizedBookingAccess",
                    "Booking",
                    bookingId,
                    false,
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

    }
}