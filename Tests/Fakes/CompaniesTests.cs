using ApplicationLayer.Companies;
using InfrastructureLayer.Persistence;
using InfrastructureLayer.Service.Companies;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Fakes;

public class CompaniesTests
{
    [Fact]
    public async Task RegisterCompanyAsync_ShouldCreateCompany_AndReturnDto()
    {
        var ownerId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        var service = new CompanyService(
            db,
            new FakeCurrentUser(ownerId),
            new FakeAuditLogger()
        );

        var request = new RegisterCompanyRequest("Test AB");

        var dto = await service.RegisterCompanyAsync(request, CancellationToken.None);

        Assert.NotNull(dto);
        Assert.Equal("Test AB", dto.Name);

        var existsInDb = await db.ShopCompanies.AnyAsync(c => c.Id == dto.Id);
        Assert.True(existsInDb);
    }

    [Fact]
    public async Task RegisterCompanyAsync_WhenNameIsEmpty_ShouldThrow()
    {
        var ownerId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        var service = new CompanyService(
            db,
            new FakeCurrentUser(ownerId),
            new FakeAuditLogger()
        );

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.RegisterCompanyAsync(
                new RegisterCompanyRequest(" "),
                CancellationToken.None
            ));
    }


}
