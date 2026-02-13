using ApplicationLayer.Features.Authorize.Queries.Login;
using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Users;
using InfrastructureLayer.Repositories;
using InfrastructureLayer.Helpers;
using Microsoft.Extensions.Configuration;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Employees;

public class InactiveEmployeeLoginTests
{
    private IJwtGenerator CreateJwtGenerator()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtSettings:Key"] = "ThisIsATestSecretKeyForJwtTokenGenerationWithAtLeast256Bits",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:ExpireMinutes"] = "60"
            }!)
            .Build();

        return new JWTGenerator(config);
    }

    [Fact]
    public async Task Login_Should_Fail_When_Employee_Is_Deactivated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var employee = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword("password123"));
        employee.Deactivate();
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var authRepo = new AuthRepository(db);
        var jwtGenerator = CreateJwtGenerator();
        var handler = new LoginQueryHandler(authRepo, jwtGenerator);

        var query = new LoginQuery("john@test.com", "password123");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("deactivated", result.ErrorMessage);
    }

    [Fact]
    public async Task Login_Should_Succeed_When_Employee_Is_Active()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var employee = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword("password123"));
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var authRepo = new AuthRepository(db);
        var jwtGenerator = CreateJwtGenerator();
        var handler = new LoginQueryHandler(authRepo, jwtGenerator);

        var query = new LoginQuery("john@test.com", "password123");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Login_Should_Succeed_After_Employee_Is_Reactivated()
    {
        // Arrange
        using var db = TestDbFactory.CreateDb();

        var employee = new User("John Employee", "john@test.com", null, UserRole.Employee);
        employee.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword("password123"));
        employee.Deactivate();
        employee.Activate(); // Reactivate
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var authRepo = new AuthRepository(db);
        var jwtGenerator = CreateJwtGenerator();
        var handler = new LoginQueryHandler(authRepo, jwtGenerator);

        var query = new LoginQuery("john@test.com", "password123");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}
