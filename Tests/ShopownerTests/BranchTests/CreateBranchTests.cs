using ApplicationLayer.Features.Branches.Commands.CreateBranch;
using InfrastructureLayer.Repositories;
using Tests.ShopownerTests.TestHelpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.ShopownerTests.BranchTests;

public class CreateBranchTests
{
    [Fact]
    public async Task CreateBranch_Should_Create_WhenOwnerOwnsCompany()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        await db.SaveChangesAsync();

        var handler = new CreateBranchCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db)
        );

        var cmd = new CreateBranchCommand(
            ShopCompanyId: company.Id,
            Name: "Branch1",
            City: "City",
            Address: "Addr"
        );

        var result = await handler.Handle(cmd, CancellationToken.None);

        var saved = db.Branches.Single();
        Assert.Equal(company.Id, saved.ShopCompanyId);
        Assert.Equal("Branch1", saved.Name);
    }

    [Fact]
    public async Task CreateBranch_WhenDuplicateNameInCompany_ShouldThrow()
    {
        using var db = TestDbFactory.CreateDb();

        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser { IsAuthenticated = true, UserId = ownerId };

        var company = ShopOwnerSeed.Company(db, ownerId, "TestCo");
        ShopOwnerSeed.Branch(db, company, name: "Branch1");
        await db.SaveChangesAsync();

        var handler = new CreateBranchCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db)
        );

        var cmd = new CreateBranchCommand(company.Id, "Branch1", "City", "Addr");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }
  
    [Fact]
    public async Task CreateBranch_WhenNotAuthenticated_ShouldThrow()
    {
        using var db = TestDbFactory.CreateDb();
        var currentUser = new FakeCurrentUser { IsAuthenticated = false };
        var handler = new CreateBranchCommandHandler(
            currentUser,
            new CompanyRepository(db),
            new BranchRepository(db)
        );
        var cmd = new CreateBranchCommand(
            ShopCompanyId: Guid.NewGuid(),
            Name: "Branch1",
            City: "City",
            Address: "Addr"
        );
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }


}
