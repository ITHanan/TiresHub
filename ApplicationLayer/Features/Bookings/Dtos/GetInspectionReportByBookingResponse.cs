namespace ApplicationLayer.Features.Bookings.DTOs
{
    public sealed class GetInspectionReportByBookingResponse
    {
        public Guid BookingId { get; init; }
        public DateTime AppointmentDate { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string ServiceType { get; init; } = string.Empty;
        public InspectionReportDto? Report { get; init; }
    }

    public sealed class InspectionReportApiResponse
    {
        public bool IsSuccess { get; init; } = true;
        public GetInspectionReportByBookingResponse Data { get; init; } = default!;
    }
}
