using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.DTOs
{
    public sealed class BookingSummaryDto
    {
        public Guid BookingId { get; init; }
        public DateTime AppointmentDate { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public ServiceType ServiceType { get; init; }
    }
}
