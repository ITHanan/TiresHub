using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Auditing
{
    public class LoginAuditLog : BaseEntity
    {
        public Guid? UserId { get; private set; }
        public string Identifier { get; private set; }
        public string Role { get; private set; }
        public bool Success { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTime Timestamp { get; private set; }

        private LoginAuditLog() { }

        public LoginAuditLog(
            Guid? userId,
            string identifier,
            string role,
            bool success,
            string? failureReason)
        {
            UserId = userId;
            Identifier = identifier;
            Role = role;
            Success = success;
            FailureReason = failureReason;
            Timestamp = DateTime.UtcNow;
        }
    }
}
