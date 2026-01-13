using DomainLayer.Common;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Communication
{
    public class CommunicationLog : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public CommunicationChannel Channel { get; private set; }
        public string Status { get; private set; }

        public CommunicationLog(Guid bookingId, CommunicationChannel channel, string status)
        {
            BookingId = bookingId;
            Channel = channel;
            Status = status;
        }
    }
}
