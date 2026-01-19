using ApplicationLayer.Warehouses;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using Tests.Fakes;
using Xunit;

namespace Tests.WarehousesTests;

public class WarehousesTests
{
    [Fact]
    public async Task CreateWarehouseAsync_ShouldCreateWarehouse()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        db.Branches.Add(new DomainLayer.Shops.Branch(
            "Branch 1", "Stockholm", "Testgatan", companyId
        )
        { Id = branchId });

        await db.SaveChangesAsync();

        var service = new WarehouseService(
            db,
            new FakeCompanyService(companyId),
            new FakeAuditLogger()
        );

        var request = new CreateWarehouseRequest(branchId, "Main Warehouse");

        // Act
        var dto = await service.CreateWarehouseAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(dto);

        var exists = await db.Warehouses.AnyAsync(w => w.Id == dto.Id);
        Assert.True(exists);
    }
}
