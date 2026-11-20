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
        public Guid? OwnerId { get; private set; }
        public string? ContactTarget { get; private set; }
        public string? Content { get; private set; }
        public string? EvidenceRefs { get; private set; }

        private CommunicationLog() { Status = string.Empty; }

        public CommunicationLog(Guid bookingId, CommunicationChannel channel, string status)
        {
            BookingId = bookingId;
            Channel = channel;
            Status = status;
        }

        public CommunicationLog(Guid bookingId, Guid ownerId, string target, CommunicationChannel channel, string content, string evidenceRefs)
            : this(bookingId, channel, "sending")
        {
            OwnerId = ownerId;
            ContactTarget = target;
            Content = content;
            EvidenceRefs = evidenceRefs;
        }

        public void Complete(bool sent) => Status = sent ? "sent" : "send_failed";
    }
}
