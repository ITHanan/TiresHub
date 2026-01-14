using DomainLayer.Common;
//this class is used to store user credentials such as password hash separately from user profile information for security reasons 
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
