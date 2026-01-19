using ApplicationLayer.Features.StaffAuth.Commands.StaffStartAuth;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests;

public class StaffStartAuthTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IVerificationCodeRepository> _codeRepo = new();

    private readonly StaffStartAuthCommandHandler _startHandler;

    public StaffStartAuthTests()
    {
        _startHandler = new StaffStartAuthCommandHandler(
            _userRepo.Object,
            _codeRepo.Object
        );
    }

    [Fact]
    public async Task StaffLogin_Fails_When_User_Does_Not_Exist()
    {
        // Arrange
        _userRepo
            .Setup(r => r.GetByIdentifierAsync("ghost@test.com"))
            .ReturnsAsync((User?)null);

        var command = new StaffStartAuthCommand("ghost@test.com");

        // Act
        var result = await _startHandler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should()
            .Contain("This account does not exist. Please contact your administrator.");
    }

    [Fact]
    public async Task StaffLogin_Fails_When_User_Is_Deactivated()
    {
        // Arrange
        var user = new User(
            name: "Manager",
            email: "manager@test.com",
            phone: null,
            role: UserRole.ShopManager
        );
        user.AssignBranch(Guid.NewGuid());
        user.Deactivate(); //the user is deactivated

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("manager@test.com"))
            .ReturnsAsync(user);

        var command = new StaffStartAuthCommand("manager@test.com");

        // Act
        var result = await _startHandler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should()
            .Contain("deactivated");
    }

    
}
