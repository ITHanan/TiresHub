using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs
{
    public class BookingDetailsDto
    {
        public Guid Id { get; set; }
        public string VehiclePlateNumber { get; set; } = default!;
        public ServiceType ServiceType { get; set; }
        public DateTime AppointmentDate { get; set; }
        public BookingStatus Status { get; set; }
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = default!;
        public Guid? WarehouseId { get; set; }
        public Guid? AssignedEmployeeId { get; set; }
    }
}
