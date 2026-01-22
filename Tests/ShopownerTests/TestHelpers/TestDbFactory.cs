using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.ShopownerTests.TestHelpers;

public static class TestDbFactory
{
    public static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
