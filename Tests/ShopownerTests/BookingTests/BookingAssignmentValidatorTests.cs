using ApplicationLayer.Features.Bookings.Validators;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using FluentAssertions;
using Xunit;

namespace Tests.ShopownerTests.BookingTests;

public class BookingAssignmentValidatorTests
{
    [Fact]
    public void HasAssignedEmployee_returns_false_when_no_employee_assigned()
    {
        // Arrange
        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TireType.Summer,
            null
        );

        // Act
        var result = BookingAssignmentValidator.HasAssignedEmployee(booking);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasAssignedEmployee_returns_true_when_employee_is_assigned()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var booking = Booking.Create(
            ServiceType.ChangeTires,
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TireType.Summer,
            null
        );
        booking.AssignEmployee(employeeId);

        // Act
        var result = BookingAssignmentValidator.HasAssignedEmployee(booking);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetMissingAssignmentErrorMessage_returns_clear_error_message()
    {
        // Act
        var message = BookingAssignmentValidator.GetMissingAssignmentErrorMessage();

        // Assert
        message.Should().NotBeNullOrWhiteSpace();
        message.Should().Contain("employee must be assigned");
        message.Should().Contain("before proceeding");
    }
}
