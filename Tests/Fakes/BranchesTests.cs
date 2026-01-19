using ApplicationLayer.Branches;
using InfrastructureLayer.Persistence;
using InfrastructureLayer.Service.Branches;
using InfrastructureLayer.Service.Companies;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Fakes;

public class BranchesTests
{
    [Fact]
    public async Task CreateBranchAsync_ShouldCreateBranch_AndReturnDto()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        // Fake ICompanyService => returnerar companyId som BranchService använder
        var companyService = new CompanyService(companyId);
        var audit = new FakeAuditLogger();

        var service = new BranchService(db, companyService, audit);

        var request = new CreateBranchRequest
        {
            Name = "Branch 1",
            City = "Stockholm",
            Address = "Testgatan 1"
        };

        // Act
        var dto = await service.CreateBranchAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Branch 1", dto.Name);
        Assert.Equal("Stockholm", dto.City);
        Assert.Equal("Testgatan 1", dto.Address);

        var exists = await db.Branches.AnyAsync(b => b.Id == dto.Id);
        Assert.True(exists);
    }

    [Fact]
    public async Task CreateBranchAsync_WhenCompanyNotFound_ShouldThrow()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        // Fake ICompanyService => null => "Company not found."
        var companyService = new CompanyService(companyId: null);
        var audit = new FakeAuditLogger();

        var service = new BranchService(db, companyService, audit);

        var request = new CreateBranchRequest
        {
            Name = "Branch 1",
            City = "Stockholm",
            Address = "Testgatan 1"
        };

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateBranchAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateBranchAsync_WhenNameCityOrAddressMissing_ShouldThrow()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        var service = new BranchService(db, new CompanyService(Guid.NewGuid()), new FakeAuditLogger());

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.CreateBranchAsync(new CreateBranchRequest
            {
                Name = "",
                City = "Stockholm",
                Address = "Testgatan 1"
            }, CancellationToken.None));
    }
}
