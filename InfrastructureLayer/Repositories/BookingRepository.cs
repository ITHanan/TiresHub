using ApplicationLayer.Interfaces;
using DomainLayer.Bookings;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        }

        public async Task<bool> BookingExistsAsync(Guid bookingId, CancellationToken ct)
        {
            return await _context.Bookings
                .AnyAsync(b => b.Id == bookingId, ct);
        }

        public async Task AddAsync(Booking booking, CancellationToken ct)
        {
            await _context.Bookings.AddAsync(booking, ct);
        }
    }
}
