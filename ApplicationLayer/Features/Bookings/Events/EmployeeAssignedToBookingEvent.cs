using MediatR;

namespace ApplicationLayer.Features.Bookings.Events;

public class EmployeeAssignedToBookingEvent : INotification
{
    public Guid EmployeeId { get; }
    public Guid BookingId { get; }

    public EmployeeAssignedToBookingEvent(Guid employeeId, Guid bookingId)
    {
        EmployeeId = employeeId;
        BookingId = bookingId;
    }
}
