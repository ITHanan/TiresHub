using ApplicationLayer.Capacity;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using Tests.Fakes;
using Xunit;

namespace Tests.CapacityTests;

public class CapacityTests
{
    [Fact]
    public async Task UpdateCapacityAsync_ShouldUpdateCapacity()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        // Seed: en branch måste finnas
        db.Branches.Add(new DomainLayer.Shops.Branch(
            name: "Branch 1",
            city: "Stockholm",
            address: "Testgatan 1",
            companyId: companyId
        )
        { Id = branchId });

        await db.SaveChangesAsync();

        var service = new CapacityService(
            db,
            new FakeCompanyService(companyId),
            new FakeAuditLogger()
        );

        // IMPORTANT:
        // Kolla UpdateCapacityRequest.cs och använd rätt ctor-params.
        var request = new UpdateCapacityRequest(branchId, 25);

        // Act
        await service.UpdateCapacityAsync(request, CancellationToken.None);

        // Assert
        var updated = await db.Branches.FirstAsync(b => b.Id == branchId);
        Assert.Equal(25, updated.Capacity);
    }

    [Fact]
    public async Task UpdateCapacityAsync_WhenCapacityIsNegative_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db-{Guid.NewGuid()}")
            .Options;

        await using var db = new AppDbContext(options);

        var service = new CapacityService(
            db,
            new FakeCompanyService(companyId),
            new FakeAuditLogger()
        );

        // IMPORTANT: matcha din UpdateCapacityRequest ctor
        var request = new UpdateCapacityRequest(Guid.NewGuid(), -1);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.UpdateCapacityAsync(request, CancellationToken.None));
    }
}
