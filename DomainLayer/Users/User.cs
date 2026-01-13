using DomainLayer.Common;
using DomainLayer.Enums;

namespace DomainLayer.Users
{
    public class User : BaseEntity
    {
        public string Name { get; private set; }
        public string UserEmail { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Phone { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }

        protected User() { }

        public User(string name, string email, string phone, UserRole role)
        {
            Name = name;
            UserEmail = email;
            Phone = phone;
            Role = role;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
