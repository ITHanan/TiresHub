using DomainLayer.Common;

namespace DomainLayer.Users
{
    public class UserCredential : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string PasswordHash { get; private set; }

        protected UserCredential() { }

        public UserCredential(Guid userId, string passwordHash)
        {
            UserId = userId;
            PasswordHash = passwordHash;
        }

        public void UpdatePassword(string newHash)
        {
            PasswordHash = newHash;
        }
    }
}
