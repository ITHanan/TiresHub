using DomainLayer.shops;
using InfrastructureLayer.Persistence;

namespace Tests.ShopownerTests.TestHelpers;

public static class ShopOwnerSeed
{
    public static ShopCompany Company(AppDbContext db, Guid ownerId, string name = "TestCo")
    {
        var company = new ShopCompany(name, ownerId); // du har nu implementerat Create()
        db.ShopCompanies.Add(company);
        return company;
    }

    public static Branch Branch(AppDbContext db, ShopCompany company, string name = "B1", string city = "City", string address = "Addr")
    {
        var branch = new Branch(name, city, address, company.Id);
        db.Branches.Add(branch);
        return branch;
    }

    public static Warehouse Warehouse(AppDbContext db, Branch branch, string name = "W1", int capacity = 10)
    {
        var warehouse = new Warehouse(name, capacity, branch.Id);
        db.Warehouses.Add(warehouse);
        return warehouse;
    }
}
