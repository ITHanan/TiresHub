using DomainLayer.Common;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Bookings
{

    public class InspectionReport : BaseEntity
    {
        // FK to Booking
        public Guid BookingId { get; private set; }

        public string? Notes { get; private set; }

        public Guid? CreatedByEmployeeId { get; private set; }
        public User? CreatedByUser{ get; private set; }


        public ICollection<InspectionPhoto> Photos { get; private set; } = new List<InspectionPhoto>();

        protected InspectionReport() { }

        public InspectionReport(Guid bookingId, string? notes, Guid? createdByEmployeeId = null)
        {
            BookingId = bookingId;
            Notes = notes;
            CreatedByEmployeeId = createdByEmployeeId;
            // CreatedAt comes from BaseEntity
        }

        public void AddPhoto(InspectionPhoto photo)
        {
            Photos.Add(photo);
        }
    }
}
