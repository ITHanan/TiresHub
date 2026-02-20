using DomainLayer.Bookings;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetAssignedBookings;

namespace ApplicationLayer.Interfaces.Bookings;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken ct);
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct);

    Task<IReadOnlyList<Booking>> GetForVehicleOwnerAsync(Guid ownerUserId, CancellationToken ct);
    Task<IReadOnlyList<Booking>> GetForManagerAsync(Guid managerUserId, CancellationToken ct);

    Task<List<BookingSummaryDto>> GetBookingSummariesByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    Task<BookingDetailsDto?> GetBookingDetailsAsync(Guid bookingId, CancellationToken cancellationToken);

    // Atomically select an available warehouse in the booking's branch, increment its usage,
    // assign it to the booking, and persist both the warehouse change and the booking.
    // Returns the reserved warehouse Id on success, or null if no warehouse was available.
    Task<Guid?> ReserveWarehouseAndAddBookingAsync(Booking booking, CancellationToken ct);

    // New: get bookings assigned to a specific employee for "My tasks"
    Task<List<AssignedBookingDto>> GetAssignedBookingsAsync(Guid employeeId, CancellationToken cancellationToken);
}
