using ApplicationLayer.Features.StaffAuth.Commands.StaffStartAuth;
using ApplicationLayer.Features.StaffAuth.Commands.StaffVerifyCode;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests;

public class StaffLoginTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IVerificationCodeRepository> _codeRepo = new();
    private readonly Mock<IJwtGenerator> _jwt = new();
    private readonly Mock<ILoginAuditRepository> _auditRepo = new();

    private readonly StaffVerifyCodeCommandHandler _handler;

    private StaffVerifyCodeCommandHandler CreateHandler()
    => new(
        _userRepo.Object,
        _codeRepo.Object,
        _jwt.Object,
        _auditRepo.Object
    );

    public StaffLoginTests()
    {
        _jwt
            .Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");

        _handler = new StaffVerifyCodeCommandHandler(
            _userRepo.Object,
            _codeRepo.Object,
            _jwt.Object,
            _auditRepo.Object
        );
    }

    [Fact]
    public async Task StaffLogin_Succeeds_For_Active_ShopManager()
    {
        // Arrange
        var branchId = Guid.NewGuid();

        var user = new User(
            name: "Manager",
            email: "manager@test.com",
            phone: null,
            role: UserRole.ShopManager
        );
        user.AssignBranch(branchId);

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("manager@test.com"))
            .ReturnsAsync(user);

        var code = new VerificationCode("manager@test.com", "123456", UserRole.ShopManager);

        _codeRepo
            .Setup(r => r.GetValidCodeAsync("manager@test.com", "123456", UserRole.ShopManager))
            .ReturnsAsync(code);

        var command = new StaffVerifyCodeCommand(
            "manager@test.com",
            "123456",
            UserRole.ShopManager
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Role.Should().Be(UserRole.ShopManager.ToString());
        result.Data.BranchId.Should().Be(branchId);
        result.Data.Token.Should().NotBeNullOrEmpty();
    }


    [Fact]
    public async Task StaffLogin_Succeeds_For_Active_Employee()
    {
        // Arrange
        var user = new User(
            name: "Employee",
            email: "employee@test.com",
            phone: null,
            role: UserRole.Employee
        );
        user.AssignBranch(Guid.NewGuid());

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("employee@test.com"))
            .ReturnsAsync(user);

        var code = new VerificationCode("employee@test.com", "654321", UserRole.Employee);

        _codeRepo
            .Setup(r => r.GetValidCodeAsync("employee@test.com", "654321", UserRole.Employee))
            .ReturnsAsync(code);

        var command = new StaffVerifyCodeCommand("employee@test.com", "654321", UserRole.Employee);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Role.Should().Be("Employee");
    }



    [Fact]
    public async Task StaffLogin_Succeeds_For_Employee()
    {
        // Arrange
        var branchId = Guid.NewGuid();

        var employee = new User(
            name: "Employee",
            email: "employee@test.com",
            phone: null,
            role: UserRole.Employee
        );

        employee.AssignBranch(branchId);

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("employee@test.com"))
            .ReturnsAsync(employee);

        var verificationCode = new VerificationCode(
            "employee@test.com",
            "654321",
            UserRole.Employee
        );

        _codeRepo
            .Setup(r => r.GetValidCodeAsync("employee@test.com", "654321", UserRole.Employee))
            .ReturnsAsync(verificationCode);

        var command = new StaffVerifyCodeCommand(
            "employee@test.com",
            "654321",
            UserRole.Employee
        );

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Role.Should().Be("Employee");
        result.Data.BranchId.Should().Be(branchId);
    }

    [Fact]
    public async Task StaffVerify_Logs_Audit_On_Success()
    {
        // Arrange
        var branchId = Guid.NewGuid();

        var manager = new User(
            name: "Manager",
            email: "manager@test.com",
            phone: null,
            role: UserRole.ShopManager
        );
        manager.AssignBranch(branchId);

        var verification = new VerificationCode("manager@test.com", "123456", UserRole.ShopManager);

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("manager@test.com"))
            .ReturnsAsync(manager);

        _codeRepo
            .Setup(r => r.GetValidCodeAsync("manager@test.com", "123456", UserRole.ShopManager))
            .ReturnsAsync(verification);

        _jwt
            .Setup(j => j.GenerateToken(manager))
            .Returns("jwt-token");

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new StaffVerifyCodeCommand("manager@test.com", "123456", UserRole.ShopManager),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _auditRepo.Verify(a =>
            a.LogAsync(
                manager.Id,
                "manager@test.com",
                "ShopManager",
                true,
                null),
            Times.Once);
    }

    [Fact]
    public async Task StaffVerify_Logs_Audit_On_Invalid_Code()
    {
        // Arrange
        _codeRepo
            .Setup(r => r.GetValidCodeAsync("emp@test.com", "000000", UserRole.Employee))
            .ReturnsAsync((VerificationCode?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new StaffVerifyCodeCommand("emp@test.com", "000000", UserRole.Employee),////
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _auditRepo.Verify(a =>
            a.LogAsync(
                null,
                "emp@test.com",
                "Unknown",
                false,
                It.Is<string>(s => s.Contains("Invalid"))),
            Times.Once);
    }
    [Fact]
    public async Task StaffVerify_Logs_Audit_When_No_Branch_Assigned()
    {
        // Arrange
        var employee = new User(
            name: "Employee",
            email: "emp@test.com",
            phone: null,
            role: UserRole.Employee
        );

        var verification = new VerificationCode("emp@test.com", "123456", UserRole.Employee);

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("emp@test.com"))
            .ReturnsAsync(employee);

        _codeRepo
            .Setup(r => r.GetValidCodeAsync("emp@test.com", "123456", UserRole.ShopManager))
            .ReturnsAsync(verification);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new StaffVerifyCodeCommand("emp@test.com", "123456", UserRole.Employee),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _auditRepo.Verify(a =>
            a.LogAsync(
                employee.Id,
                "emp@test.com",
                "Employee",
                false,
                It.Is<string>(s => s.Contains("branch"))),
            Times.Once);
    }



}
