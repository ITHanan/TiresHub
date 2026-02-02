using DomainLayer.Bookings;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;

namespace ApplicationLayer.Interfaces.Bookings;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken ct);
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct);

    Task<IReadOnlyList<Booking>> GetForVehicleOwnerAsync(Guid ownerUserId, CancellationToken ct);
    Task<IReadOnlyList<Booking>> GetForManagerAsync(Guid managerUserId, CancellationToken ct);

    Task<List<BookingSummaryDto>> GetBookingSummariesByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    Task<BookingDetailsDto?> GetBookingDetailsAsync(Guid bookingId, CancellationToken cancellationToken);

}
