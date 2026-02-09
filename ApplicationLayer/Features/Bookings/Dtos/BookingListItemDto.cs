using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.DTOs;

public sealed class BookingListItemDto
{
    public Guid BookingId { get; init; }
    public DateTime AppointmentDate { get; init; }
    public ServiceType ServiceType { get; init; }
    public BookingStatus Status { get; init; }

    public string VehiclePlateNumber { get; init; } = "";
    public string BranchName { get; init; } = "";

    
}
