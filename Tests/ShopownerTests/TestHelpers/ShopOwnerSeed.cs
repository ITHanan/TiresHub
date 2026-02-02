using DomainLayer.Bookings;
using DomainLayer.Enums;
using DomainLayer.shops;
using DomainLayer.Users;
using DomainLayer.Vehicles;
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

    public static User ShopManager(AppDbContext db, Branch branch, string name = "Manager1", string email = "manager@test.com")
    {
        var user = new User(name, email, null, UserRole.ShopManager);
        user.AssignBranch(branch.Id);
        user.SetPasswordHash("hashedpassword");
        db.Users.Add(user);
        return user;
    }

    public static Vehicle Vehicle(AppDbContext db, Guid ownerId, string plateNumber = "ABC123")
    {
        var vehicle = new Vehicle(plateNumber, ownerId);
        db.Vehicles.Add(vehicle);
        return vehicle;
    }


    public static Booking Booking(
        AppDbContext db,
        Branch branch,
        Vehicle vehicle,
        ServiceType serviceType = ServiceType.ChangeTires,
        DateTime? appointmentDate = null,
        TireType tireType = TireType.Summer,
        int? quantity = null)
    {
        var date = appointmentDate ?? DateTime.UtcNow.AddDays(1);

        if (serviceType == ServiceType.BuyNewTires)
            quantity ??= 4;

        var booking = DomainLayer.Bookings.Booking.Create(
            serviceType,
            date,
            vehicle.Id,
            branch.Id,
            tireType,
            quantity
        );

        db.Bookings.Add(booking);
        return booking;
    }


}
