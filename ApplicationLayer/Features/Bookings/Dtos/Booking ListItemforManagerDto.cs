using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Bookings.Dtos
{
    public sealed class BookingListItemForManagerDto
    {
        public Guid BookingId { get; init; }
        public DateTime AppointmentDate { get; init; }
        public ServiceType ServiceType { get; init; }
        public BookingStatus Status { get; init; }

        public string VehiclePlateNumber { get; init; } = "";
        public string BranchName { get; init; } = "";

        // New: assignment info to be visible in list views for shop staff
        public Guid? WarehouseId { get; init; }
        public Guid? AssignedEmployeeId { get; init; }
    }
}
