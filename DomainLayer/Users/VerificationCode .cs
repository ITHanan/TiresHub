using DomainLayer.Common;

namespace DomainLayer.Users
{
    public class VerificationCode : BaseEntity
    {
        public string Identifier { get; private set; } // email or phone
        public string Code { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool Used { get; private set; }

        protected VerificationCode() { }

        public VerificationCode(string identifier, string code)
        {
            Identifier = identifier;
            Code = code;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddMinutes(5);
            Used = false;
        }
        // Checks if the code is still valid (not used and not expired)
        public bool IsValid()
            => !Used && DateTime.UtcNow <= ExpiresAt;

        
        // Marks the code as used
        public void MarkAsUsed()
        {
            Used = true;
        }
    }
}
