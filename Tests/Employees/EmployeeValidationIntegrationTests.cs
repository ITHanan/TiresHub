using ApplicationLayer.Features.Employees.Commands;
using ApplicationLayer.Features.Employees.Validators;
using FluentValidation;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Employees;

/// <summary>
/// Tests to verify FluentValidation integration with MediatR pipeline
/// </summary>
public class EmployeeValidationIntegrationTests
{
    [Fact]
    public async Task CreateEmployeeValidator_Should_Be_Registered_And_Validate()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var invalidCommand = new CreateEmployeeCommand(
            Name: "",
            Email: null,
            Phone: null
        );

        // Act
        var result = await validator.ValidateAsync(invalidCommand);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Name is required"));
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Email or phone is required"));
    }

    [Fact]
    public async Task DeactivateEmployeeValidator_Should_Be_Registered_And_Validate()
    {
        // Arrange
        var validator = new DeactivateEmployeeCommandValidator();
        var invalidCommand = new DeactivateEmployeeCommand(Guid.Empty);

        // Act
        var result = await validator.ValidateAsync(invalidCommand);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Employee ID is required"));
    }

    [Fact]
    public async Task ReactivateEmployeeValidator_Should_Be_Registered_And_Validate()
    {
        // Arrange
        var validator = new ReactivateEmployeeCommandValidator();
        var invalidCommand = new ReactivateEmployeeCommand(Guid.Empty);

        // Act
        var result = await validator.ValidateAsync(invalidCommand);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Employee ID is required"));
    }

    [Fact]
    public async Task CreateEmployeeValidator_Should_Pass_With_Valid_Data()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var validCommand = new CreateEmployeeCommand(
            Name: "John Doe",
            Email: "john@example.com",
            Phone: "0700000000"
        );

        // Act
        var result = await validator.ValidateAsync(validCommand);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
