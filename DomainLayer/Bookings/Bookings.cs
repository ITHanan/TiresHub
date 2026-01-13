using DomainLayer.Common;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Bookings
{
    public class Booking : BaseEntity
    {
        public ServiceType ServiceType { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTime AppointmentDate { get; private set; }

        public Guid VehicleId { get; private set; }
        public Guid BranchId { get; private set; }
        public Guid? WarehouseId { get; private set; }
        public Guid? AssignedEmployeeId { get; private set; }

        protected Booking() { }

        public Booking(ServiceType serviceType, DateTime appointmentDate, Guid vehicleId, Guid branchId)
        {
            ServiceType = serviceType;
            AppointmentDate = appointmentDate;
            VehicleId = vehicleId;
            BranchId = branchId;
            Status = BookingStatus.Confirmed;
        }

        public void AssignWarehouse(Guid warehouseId) => WarehouseId = warehouseId;
        public void AssignEmployee(Guid employeeId) => AssignedEmployeeId = employeeId;

        public void Complete() => Status = BookingStatus.Completed;
        public void Cancel() => Status = BookingStatus.Cancelled;
    }
}
