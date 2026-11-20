namespace ApplicationLayer.Features.Bookings.DTOs
{
    public sealed class InspectionReportDto
    {
        public Guid Id { get; init; }
        public string? Notes { get; init; }
        public List<InspectionReportPhotoDto> Photos { get; init; } = new();
        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }
    }

    public sealed class InspectionReportPhotoDto
    {
        public Guid Id { get; init; }
        public string ImageUrl { get; init; } = string.Empty;
    }
}
