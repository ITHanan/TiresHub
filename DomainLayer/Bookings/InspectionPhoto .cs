using DomainLayer.Common;
using System;

namespace DomainLayer.Bookings
{
    public class InspectionPhoto : BaseEntity
    {
        public Guid InspectionReportId { get; private set; }
        public string ImageUrl { get; private set; } = default!;

        protected InspectionPhoto() { }

        public InspectionPhoto(Guid reportId, string imageUrl)
        {
            InspectionReportId = reportId;
            ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
            // CreatedAt is inherited from BaseEntity
        }
    }
}
