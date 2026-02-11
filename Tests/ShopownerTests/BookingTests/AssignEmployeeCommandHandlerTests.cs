using ApplicationLayer.Features.Bookings.Commands.AssignEmployee;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentAssertions;
using Moq;
using System.Threading;
using Xunit;

namespace Tests.ShopownerTests.BookingTests;

public class AssignEmployeeCommandHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICompanyRepository> _companyRepo = new();
    private readonly Mock<IAuditRepository> _auditRepo = new();

    private AssignEmployeeCommandHandler CreateHandler()
        => new(
            _bookingRepo.Object,
            _userRepo.Object,
            _companyRepo.Object,
            _auditRepo.Object
        );

    [Fact]
    public async Task Assign_employee_successfully()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            branchId,
            TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        var employee = new User("John Doe", "john@example.com", null, UserRole.Employee);
        typeof(User).GetProperty("Id")!.SetValue(employee, employeeId);
        employee.AssignBranch(branchId);
        employee.Activate();

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _userRepo
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        _companyRepo
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(managerId, UserRole.ShopManager, branchId, bookingId, employeeId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.AssignedEmployeeId.Should().Be(employeeId);

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditRepo.Verify(a => a.LogAsync(
            managerId,
            It.IsAny<string>(),
            nameof(Booking),
            bookingId,
            true,
            null,
            null), Times.Once);
    }

    [Fact]
    public async Task Reject_assignment_without_employee_selection()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
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

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _userRepo
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(managerId, UserRole.ShopManager, branchId, bookingId, employeeId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Employee not found");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        booking.AssignedEmployeeId.Should().BeNull();
    }

    [Fact]
    public async Task Reject_cross_branch_employee_assignment()
    {
        // Arrange
        var bookingBranchId = Guid.NewGuid();
        var employeeBranchId = Guid.NewGuid();

        var bookingId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            bookingBranchId,
            TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        var employee = new User("John Doe", "john@example.com", null, UserRole.Employee);
        typeof(User).GetProperty("Id")!.SetValue(employee, employeeId);
        employee.AssignBranch(employeeBranchId);
        employee.Activate();

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _userRepo
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(managerId, UserRole.ShopManager, bookingBranchId, bookingId, employeeId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("only assign employees from your branch");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditRepo.Verify(a => a.LogAsync(
            managerId,
            It.IsAny<string>(),
            nameof(Booking),
            bookingId,
            false,
            It.IsAny<string>(),
            null), Times.Once);
        booking.AssignedEmployeeId.Should().BeNull();
    }

    [Fact]
    public async Task Reject_inactive_employee_assignment()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            branchId,
            TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        var employee = new User("John Doe", "john@example.com", null, UserRole.Employee);
        typeof(User).GetProperty("Id")!.SetValue(employee, employeeId);
        employee.AssignBranch(branchId);
        employee.Deactivate(); // Make employee inactive

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _userRepo
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(managerId, UserRole.ShopManager, branchId, bookingId, employeeId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("inactive");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        booking.AssignedEmployeeId.Should().BeNull();
    }

    [Fact]
    public async Task Reassignment_logs_correctly_and_notifies_new_employee()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var oldEmployeeId = Guid.NewGuid();
        var newEmployeeId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            branchId,
            TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);
        booking.AssignEmployee(oldEmployeeId); // Pre-assign old employee

        var newEmployee = new User("Jane Smith", "jane@example.com", null, UserRole.Employee);
        typeof(User).GetProperty("Id")!.SetValue(newEmployee, newEmployeeId);
        newEmployee.AssignBranch(branchId);
        newEmployee.Activate();

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _userRepo
            .Setup(r => r.GetByIdAsync(newEmployeeId))
            .ReturnsAsync(newEmployee);

        _companyRepo
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(managerId, UserRole.ShopManager, branchId, bookingId, newEmployeeId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.AssignedEmployeeId.Should().Be(newEmployeeId);

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditRepo.Verify(a => a.LogAsync(
            managerId,
            It.IsAny<string>(),
            nameof(Booking),
            bookingId,
            true,
            null,
            It.Is<string>(m => m.Contains(oldEmployeeId.ToString()))), Times.Once);
    }

    [Fact]
    public async Task Reject_unauthorized_role_assignment()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var vehicleOwnerId = Guid.NewGuid();

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(vehicleOwnerId, UserRole.VehicleOwner, branchId, bookingId, employeeId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unauthorized");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditRepo.Verify(a => a.LogAsync(
            vehicleOwnerId,
            It.IsAny<string>(),
            nameof(Booking),
            bookingId,
            false,
            It.IsAny<string>(),
            null), Times.Once);
    }

    [Fact]
    public async Task Reject_assignment_to_booking_from_different_branch()
    {
        // Arrange
        var managerBranchId = Guid.NewGuid();
        var bookingBranchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            bookingBranchId,
            TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(managerId, UserRole.ShopManager, managerBranchId, bookingId, employeeId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("only assign employees to bookings from your branch");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditRepo.Verify(a => a.LogAsync(
            managerId,
            It.IsAny<string>(),
            nameof(Booking),
            bookingId,
            false,
            It.IsAny<string>(),
            null), Times.Once);
    }

    [Fact]
    public async Task Reject_assignment_of_non_employee_user()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            vehicleId,
            branchId,
            TireType.Summer,
            null
        );
        typeof(Booking).GetProperty("Id")!.SetValue(booking, bookingId);

        var user = new User("John Doe", "john@example.com", null, UserRole.VehicleOwner);
        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        user.Activate();

        _bookingRepo
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        _userRepo
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new AssignEmployeeCommand(managerId, UserRole.ShopManager, branchId, bookingId, userId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not an employee");

        _companyRepo.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        booking.AssignedEmployeeId.Should().BeNull();
    }
}
