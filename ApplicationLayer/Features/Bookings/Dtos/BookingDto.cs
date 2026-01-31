using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.Dtos
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public ServiceType ServiceType { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime AppointmentDate { get; set; }
        public Guid VehicleId { get; set; }
        public Guid BranchId { get; set; }
        public Guid? WarehouseId { get; set; }
        public Guid? AssignedEmployeeId { get; set; }
    }
}
