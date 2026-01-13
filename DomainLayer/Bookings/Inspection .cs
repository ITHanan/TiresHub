using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Bookings
{
    public class Inspection : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public Guid WarehouseId { get; private set; }
        public Guid EmployeeId { get; private set; }

        public Inspection(Guid bookingId, Guid warehouseId, Guid employeeId)
        {
            BookingId = bookingId;
            WarehouseId = warehouseId;
            EmployeeId = employeeId;
        }
    }
}
