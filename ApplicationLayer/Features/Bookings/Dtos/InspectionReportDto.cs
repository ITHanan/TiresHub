namespace ApplicationLayer.Features.Bookings.DTOs
{
    public sealed class InspectionReportDto
    {
        public string? Notes { get; init; }
        public List<string> Photos { get; init; } = new List<string>();
        public DateTime CreatedAt { get; init; }
        public string? CreatedByName { get; init; }
    }
}
