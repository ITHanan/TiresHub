using ApplicationLayer.Features.StartAuth.Commands.VerifyCode;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests;

public class VerifyCodeCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IVerificationCodeRepository> _codes = new();
    private readonly Mock<IJwtGenerator> _jwt = new();

    private VerifyCodeCommandHandler CreateHandler()
        => new(_users.Object, _codes.Object, _jwt.Object);


    [Fact]
    public async Task VerifyCode_Fails_When_Code_Is_Invalid()
    {
        // Arrange
        var handler = CreateHandler();

        _codes.Setup(c =>
            c.GetValidCodeAsync("test@email.com", "123456"))
            .ReturnsAsync((VerificationCode?)null);

        var command = new VerifyCodeCommand("test@email.com", "123456");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should()
     .Be("Invalid or expired verification code.");
    }

    [Fact]
    public async Task VerifyCode_Fails_When_Code_Is_Expired()
    {
        // Arrange
        var expiredCode = new VerificationCode("test@email.com", "123456");
        expiredCode.MarkAsUsed(); // simulate invalid/expired

        var handler = CreateHandler();

        _codes.Setup(c =>
            c.GetValidCodeAsync("test@email.com", "123456"))
            .ReturnsAsync((VerificationCode?)null);

        var command = new VerifyCodeCommand("test@email.com", "123456");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyCode_Creates_New_User_On_First_Login()
    {
        // Arrange
        var verification = new VerificationCode("new@email.com", "123456");

        _codes.Setup(c =>
            c.GetValidCodeAsync("new@email.com", "123456"))
            .ReturnsAsync(verification);

        _users.Setup(u =>
            u.GetByIdentifierAsync("new@email.com"))
            .ReturnsAsync((User?)null);

        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var handler = CreateHandler();
        var command = new VerifyCodeCommand("new@email.com", "123456");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsFirstLogin.Should().BeTrue();

        _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task VerifyCode_Authenticates_Existing_User()
    {
        // Arrange
        var existingUser = new User(
            name: "Sara",
            email: "sara@email.com",
            phone: "",
            role: UserRole.VehicleOwner
        );

        var verification = new VerificationCode("sara@email.com", "123456");

        _codes.Setup(c =>
            c.GetValidCodeAsync("sara@email.com", "123456"))
            .ReturnsAsync(verification);

        _users.Setup(u =>
            u.GetByIdentifierAsync("sara@email.com"))
            .ReturnsAsync(existingUser);

        _jwt.Setup(j => j.GenerateToken(existingUser))
            .Returns("jwt-token");

        var handler = CreateHandler();
        var command = new VerifyCodeCommand("sara@email.com", "123456");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.IsFirstLogin.Should().BeFalse();

        _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task VerifyCode_Marks_Code_As_Used()
    {
        // Arrange
        var verification = new VerificationCode("test@email.com", "123456");

        _codes.Setup(c =>
            c.GetValidCodeAsync("test@email.com", "123456"))
            .ReturnsAsync(verification);

        _users.Setup(u =>
            u.GetByIdentifierAsync("test@email.com"))
            .ReturnsAsync((User?)null);

        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var handler = CreateHandler();
        var command = new VerifyCodeCommand("test@email.com", "123456");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        verification.Used.Should().BeTrue();
    }

}
