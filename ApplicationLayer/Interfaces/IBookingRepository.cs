using DomainLayer.Bookings;

namespace ApplicationLayer.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct);
        Task<bool> BookingExistsAsync(Guid bookingId, CancellationToken ct);
        Task AddAsync(Booking booking, CancellationToken ct);
    }
}
