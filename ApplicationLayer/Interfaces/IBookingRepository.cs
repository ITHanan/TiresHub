using DomainLayer.Bookings;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;

namespace ApplicationLayer.Interfaces
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetBookingsByBranchIdAsync(Guid branchId);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId);
        Task<List<BookingSummaryDto>> GetBookingSummariesByBranchIdAsync(Guid branchId);
        Task<BookingDetailsDto?> GetBookingDetailsAsync(Guid bookingId);
        Task SaveChangesAsync();
    }
}
