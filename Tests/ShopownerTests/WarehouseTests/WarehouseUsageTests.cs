using ApplicationLayer.Features.Warehouses.Commands.Usage;
using DomainLayer.shops;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;

namespace Tests.ShopownerTests.WarehouseTests;

public class WarehouseUsageTests
{
    [Fact]
    public async Task IncreaseUsage_Should_Increment()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var company = new ShopCompany("TestCo", ownerId);

        db.ShopCompanies.Add(company);

        var branch = new Branch("B1", "City", "Addr", company.Id);
        db.Branches.Add(branch);

        var warehouse = new Warehouse("W1", capacity: 2, branchId: branch.Id);
        db.Warehouses.Add(warehouse);

        await db.SaveChangesAsync();

        var handler = new IncreaseWarehouseUsageCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db),
            new WarehouseRepository(db)
        );

        await handler.Handle(new IncreaseWarehouseUsageCommand(warehouse.Id), CancellationToken.None);

        var reloaded = db.Warehouses.Single(w => w.Id == warehouse.Id);
        Xunit.Assert.Equal(1, reloaded.CurrentUsage);
    }

    [Fact]
    public async Task IncreaseUsage_WhenFull_ShouldThrow()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var company = new ShopCompany("TestCo", ownerId);

        db.ShopCompanies.Add(company);

        var branch = new Branch("B1", "City", "Addr", company.Id);
        db.Branches.Add(branch);

        var warehouse = new Warehouse("W1", capacity: 1, branchId: branch.Id);
        db.Warehouses.Add(warehouse);

        await db.SaveChangesAsync();

        var handler = new IncreaseWarehouseUsageCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db),
            new WarehouseRepository(db)
        );

        await handler.Handle(new IncreaseWarehouseUsageCommand(warehouse.Id), CancellationToken.None);

        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new IncreaseWarehouseUsageCommand(warehouse.Id), CancellationToken.None));
    }
}
