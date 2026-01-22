using ApplicationLayer.Features.Warehouses.Commands.CreateWarehouse;
using DomainLayer.shops;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;

namespace Tests.ShopownerTests.WarehouseTests;

public class CreateWarehouseTests
{
    [Fact]
    public async Task CreateWarehouse_Should_Create_WhenOwnerOwnsBranch()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        // ⚠️ Skapa company med rätt OwnerId (anpassa till din ctor/factory)
        var company = new ShopCompany("TestCo", ownerId); // byt om du inte har Create()
        db.ShopCompanies.Add(company);

        var branch = new Branch("Branch1", "City", "Address", company.Id);
        db.Branches.Add(branch);

        await db.SaveChangesAsync();

        var handler = new CreateWarehouseCommandHandler(
            new BranchRepository(db),
            new WarehouseRepository(db),
            new CompanyRepository(db),
            currentUser
        );

        var cmd = new CreateWarehouseCommand(
            BranchId: branch.Id,
            Name: "Main Warehouse",
            Capacity: 100
        );

        var result = await handler.Handle(cmd, CancellationToken.None);

        Xunit.Assert.Equal(branch.Id, result.BranchId);
        Xunit.Assert.Equal("Main Warehouse", result.Name);
        Xunit.Assert.Equal(100, result.Capacity);
    }
}
