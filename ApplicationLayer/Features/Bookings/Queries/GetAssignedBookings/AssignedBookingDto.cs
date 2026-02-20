using DomainLayer.Enums;
using System;

namespace ApplicationLayer.Features.Bookings.Queries.GetAssignedBookings;

public sealed class AssignedBookingDto
{
    public Guid BookingId { get; init; }
    public DateTime AppointmentDate { get; init; }
    public ServiceType ServiceType { get; init; }

    // Warehouse display value required by UC-16
    public string WarehouseLocation { get; init; } = "";

    // Optionally show branch/shop
    public string BranchName { get; init; } = "";
}