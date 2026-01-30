using DomainLayer.Bookings;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;

namespace ApplicationLayer.Interfaces
{
    public interface IBookingRepository
    {
        Task<List<BookingSummaryDto>> GetBookingSummariesByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
        Task<BookingDetailsDto?> GetBookingDetailsAsync(Guid bookingId, CancellationToken cancellationToken = default);
    }
}
