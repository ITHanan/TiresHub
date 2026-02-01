using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.DTOs;

public sealed class BookingConfirmationDto
{
    public Guid BookingId { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime AppointmentDate { get; init; }
    public ServiceType ServiceType { get; init; }

    // extra som UI brukar vilja ha:
    public string VehiclePlateNumber { get; init; } = "";
    public string BranchName { get; init; } = "";
}
