using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs
{
    public class BookingDetailsDto
    {
        public Guid Id { get; set; }
        public string VehiclePlateNumber { get; set; } = default!;

        public ServiceType ServiceType { get; set; }

        public DateTime AppointmentDate { get; set; }
        public BookingStatus Status { get; set; } = default!;

        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = default!;

        public Guid WarehouseId { get; set; }   
        public Guid AssignedEmployeeId { get; set; }
    }
}
