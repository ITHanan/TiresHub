using ApplicationLayer.Features.StaffAuth.Commands.StaffVerifyCode;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Users;
using FluentAssertions;
using Moq;
using Xunit;

public class StaffAuthenticationScopeTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IVerificationCodeRepository> _codeRepo = new();
    private readonly Mock<IJwtGenerator> _jwt = new();
    private readonly Mock<ILoginAuditRepository> _audit = new();

    private StaffVerifyCodeCommandHandler CreateHandler()
        => new(
            _userRepo.Object,
            _codeRepo.Object,
            _jwt.Object,
            _audit.Object
        );

    [Fact]
    public async Task StaffVerify_Fails_When_Manager_Has_No_Branch()
    {
        var manager = new User(
            name: "Manager",
            email: "manager@test.com",
            phone: null,
            role: UserRole.ShopManager
        );

        var verification = new VerificationCode("manager@test.com", "123456", UserRole.ShopManager);

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("manager@test.com"))
            .ReturnsAsync(manager);

        _codeRepo
            .Setup(r => r.GetValidCodeAsync("manager@test.com", "123456", UserRole.ShopManager))
            .ReturnsAsync(verification);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new StaffVerifyCodeCommand("manager@test.com", "123456", UserRole.ShopManager),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should()
            .Contain("not assigned to any branch");
    }

    [Fact]
    public async Task StaffVerify_Fails_When_Employee_Has_No_Branch()
    {
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
            .Setup(r => r.GetValidCodeAsync("emp@test.com", "123456", UserRole.Employee))
            .ReturnsAsync(verification);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new StaffVerifyCodeCommand("emp@test.com", "123456", UserRole.Employee),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should()
            .Contain("not assigned to any branch");
    }

    [Fact]
    public async Task StaffVerify_Succeeds_For_Manager_With_Branch()
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
            .Returns("fake-jwt-token");

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new StaffVerifyCodeCommand("manager@test.com", "123456", UserRole.ShopManager),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Role.Should().Be("ShopManager");
        result.Data.BranchId.Should().Be(branchId);
        result.Data.Token.Should().Be("fake-jwt-token");
    }
    [Fact]
    public async Task StaffVerify_Succeeds_For_Employee_With_Branch()
    {
        // Arrange
        var branchId = Guid.NewGuid();

        var employee = new User(
            name: "Employee",
            email: "emp@test.com",
            phone: null,
            role: UserRole.Employee
        );
        employee.AssignBranch(branchId);

        var verification = new VerificationCode("emp@test.com", "654321",UserRole.Employee);

        _userRepo
            .Setup(r => r.GetByIdentifierAsync("emp@test.com"))
            .ReturnsAsync(employee);

        _codeRepo
            .Setup(r => r.GetValidCodeAsync("emp@test.com", "654321", UserRole.Employee))
            .ReturnsAsync(verification);

        _jwt
            .Setup(j => j.GenerateToken(employee))
            .Returns("fake-employee-token");

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new StaffVerifyCodeCommand("emp@test.com", "654321", UserRole.Employee),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Role.Should().Be("Employee");
        result.Data.BranchId.Should().Be(branchId);
        result.Data.Token.Should().Be("fake-employee-token");
    }

    [Fact]
    public async Task StaffVerify_Logs_Audit_On_Success()
    {
        var branchId = Guid.NewGuid();

        var manager = new User(
            name: "Manager",
            email: "manager@test.com",
            phone: null,
            role: UserRole.ShopManager
        );
        manager.AssignBranch(branchId);

        var verification = new VerificationCode("manager@test.com", "123456", UserRole.ShopManager);

        _userRepo.Setup(r => r.GetByIdentifierAsync("manager@test.com"))
            .ReturnsAsync(manager);

        _codeRepo.Setup(r => r.GetValidCodeAsync("manager@test.com", "123456", UserRole.ShopManager))
            .ReturnsAsync(verification);

        _jwt.Setup(j => j.GenerateToken(manager))
            .Returns("jwt");

        var handler = CreateHandler();

        await handler.Handle(
            new StaffVerifyCodeCommand("manager@test.com", "123456", UserRole.ShopManager),
            CancellationToken.None);

        _audit.Verify(a => a.LogAsync(
            manager.Id,
            "manager@test.com",
            "ShopManager",
            true,
            null),
            Times.Once);
    }
    [Fact]
    public async Task StaffVerify_Logs_Audit_On_Failure_When_No_Branch()
    {
        var employee = new User(
            name: "Employee",
            email: "emp@test.com",
            phone: null,
            role: UserRole.Employee
        );

        var verification = new VerificationCode("emp@test.com", "123456", UserRole.Employee);

        _userRepo.Setup(r => r.GetByIdentifierAsync("emp@test.com"))
            .ReturnsAsync(employee);

        _codeRepo.Setup(r => r.GetValidCodeAsync("emp@test.com", "123456", UserRole.Employee))
            .ReturnsAsync(verification);

        var handler = CreateHandler();

        await handler.Handle(
            new StaffVerifyCodeCommand("emp@test.com", "123456", UserRole.Employee),
            CancellationToken.None);

        _audit.Verify(a => a.LogAsync(
            employee.Id,
            "emp@test.com",
            "Employee",
            false,
            "No branch assigned"),
            Times.Once);
    }


}
