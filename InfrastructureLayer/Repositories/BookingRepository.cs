using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Bookings;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    public BookingRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Booking booking, CancellationToken ct)
    {
        await _db.Bookings.AddAsync(booking, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct)
    {
        return _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);
    }

    public async Task<IReadOnlyList<Booking>> GetForVehicleOwnerAsync(Guid ownerUserId, CancellationToken ct)
    {
        // Antag: Vehicle har OwnerUserId (ändra om du har annan modell)
        return await _db.Bookings
            .AsNoTracking()
            .Where(b => _db.Vehicles.Any(v => v.Id == b.VehicleId && v.OwnerId == ownerUserId))
            .OrderBy(b => b.AppointmentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetForManagerAsync(
        Guid managerUserId,
        CancellationToken ct)
    {
        return await _db.Bookings
            .AsNoTracking()
            .Where(b => _db.Branches.Any(br =>
                br.Id == b.BranchId &&
                br.Employees.Any(e => e.Id == managerUserId)))
            .OrderBy(b => b.AppointmentDate)
            .ToListAsync(ct);
    }
}
