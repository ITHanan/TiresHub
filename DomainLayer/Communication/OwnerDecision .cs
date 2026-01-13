using DomainLayer.Common;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Communication
{
    public class OwnerDecision : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public OwnerDecisionType Decision { get; private set; }

        public OwnerDecision(Guid bookingId, OwnerDecisionType decision)
        {
            BookingId = bookingId;
            Decision = decision;
        }
    }
}
