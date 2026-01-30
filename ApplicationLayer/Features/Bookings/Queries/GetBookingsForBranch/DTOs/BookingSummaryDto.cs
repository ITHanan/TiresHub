using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs
{
    public class BookingSummaryDto
    {
        public Guid Id { get; set; }
        public string VehiclePlateNumber { get; set; } = default!;
        public ServiceType ServiceType { get; set; }
        public DateTime AppointmentDate { get; set; }
        public BookingStatus Status { get; set; }
        public Guid BranchId { get; set; }
    }
}
