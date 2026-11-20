using ApplicationLayer.Features.OwnerCommunication;
using DomainLayer.Bookings;
using DomainLayer.Communication;
using DomainLayer.Users;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories;

public sealed class OwnerCommunicationRepository : IOwnerCommunicationRepository
{
    private readonly AppDbContext _db;
    public OwnerCommunicationRepository(AppDbContext db) => _db = db;
    public Task<Booking?> GetBookingAsync(Guid id, CancellationToken ct) => _db.Bookings.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<User?> GetOwnerAsync(Guid vehicleId, CancellationToken ct) => _db.Vehicles.Where(v => v.Id == vehicleId)
        .Join(_db.Users, v => v.OwnerId, u => u.Id, (v, u) => u).FirstOrDefaultAsync(ct);
    public Task<InspectionReport?> GetReportAsync(Guid bookingId, CancellationToken ct) => _db.InspectionReports
        .Include(x => x.Photos).FirstOrDefaultAsync(x => x.BookingId == bookingId, ct);
    public async Task AddLogAsync(CommunicationLog log, CancellationToken ct) => await _db.CommunicationLogs.AddAsync(log, ct);
    public Task SaveAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    public async Task<IReadOnlyList<CommunicationLog>> GetLogsAsync(Guid bookingId, CancellationToken ct) => await _db.CommunicationLogs
        .AsNoTracking().Where(x => x.BookingId == bookingId).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).ToListAsync(ct);
}
