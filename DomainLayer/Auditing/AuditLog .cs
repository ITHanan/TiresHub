using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Auditing
{
    public class AuditLog : BaseEntity
    {
        public Guid? ActorUserId { get; private set; }
        public string Action { get; private set; }
        public string EntityType { get; private set; }
        public Guid? EntityId { get; private set; }

        public bool Success { get; private set; }
        public string? Reason { get; private set; }
        public string? Metadata { get; private set; }

        protected AuditLog() { }

        public AuditLog(
            Guid? actorUserId,
            string action,
            string entityType,
            Guid? entityId,
            bool success,
            string? reason = null,
            string? metadata = null)
        {
            ActorUserId = actorUserId;
            Action = action;
            EntityType = entityType;
            EntityId = entityId;
            Success = success;
            Reason = reason;
            Metadata = metadata;
        }


    }
}
