using ApplicationLayer.Features.StartAuth.Commands;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using FluentAssertions;
using Moq;
using System.Reflection.Metadata;
using Xunit;

namespace Tests;

public class Self_registrationBlockedForStaffRoles

{
    private readonly Mock<IVerificationCodeRepository> _codeRepo = new();
    private readonly StartAuthCommandHandler _startHandler;

    public Self_registrationBlockedForStaffRoles()
    {
        _startHandler = new StartAuthCommandHandler(_codeRepo.Object);
    }

    [Fact]
    public async Task StartAuth_Blocks_ShopManager()
    {
        // Arrange
        var command = new StartAuthCommand(
            "manager@test.com",
            UserRole.ShopManager
        );

        // Act
        var result = await _startHandler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should()
            .Contain("cannot be registered directly");
    }




    [Fact]
    public async Task StartAuth_Blocks_Employee()
    {
        // Arrange
        var command = new StartAuthCommand(
            "employee@test.com",
            UserRole.Employee
        );

        // Act
        var result = await _startHandler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should()
            .Contain("cannot be registered directly");
    }


}
