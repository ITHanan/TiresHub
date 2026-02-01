using DomainLayer.Bookings;

namespace ApplicationLayer.Interfaces.Bookings;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken ct);
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct);

    Task<IReadOnlyList<Booking>> GetForVehicleOwnerAsync(Guid ownerUserId, CancellationToken ct);
    Task<IReadOnlyList<Booking>> GetForManagerAsync(Guid managerUserId, CancellationToken ct);


}
