using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Bookings
{

    public class InspectionReport : BaseEntity
    {
        public Guid InspectionId { get; private set; }
        public string? Notes { get; private set; }

        public ICollection<InspectionPhoto> Photos { get; private set; } = new List<InspectionPhoto>();

        public InspectionReport(Guid inspectionId, string? notes)
        {
            InspectionId = inspectionId;
            Notes = notes;
        }
    }
}
