namespace ApplicationLayer.Features.Bookings.DTOs
{
    public sealed class GetInspectionReportByBookingResponse
    {
        public BookingSummaryDto Booking { get; init; } = default!;
        public InspectionReportDto? Report { get; init; }
        public string? OwnerDecision { get; init; }
    }
}
