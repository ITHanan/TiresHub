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
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public Guid ActorUserId { get; private set; }
        public string Action { get; private set; }
        public string EntityType { get; private set; }
        public Guid EntityId { get; private set; }
        public string UserId { get; }
        public string TargetType { get; }
        public Guid Guid { get; }

        public AuditLog(Guid actorUserId, string action, string entityType, Guid entityId)
        {
            ActorUserId = actorUserId;
            Action = action;
            EntityType = entityType;
            EntityId = entityId;
        }

        public AuditLog(string userId, string action, string targetType, Guid guid)
        {
            UserId = userId;
            Action = action;
            TargetType = targetType;
            Guid = guid;
        }
    }
}
