using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Bookings
{
    public class InspectionPhoto : BaseEntity
    {
        public Guid InspectionReportId { get; private set; }
        public string ImageUrl { get; private set; }
        public InspectionPhoto()
        {
        }
        public InspectionPhoto(Guid reportId, string imageUrl)
        {
            InspectionReportId = reportId;
            ImageUrl = imageUrl;
        }
    }
}
