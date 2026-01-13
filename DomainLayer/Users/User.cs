using DomainLayer.Common;
using DomainLayer.Enums;

namespace DomainLayer.Users
{
    public class User : BaseEntity
    {
        public  string  Name { get; private set; }= default!;
        public string UserEmail { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Phone { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }

        protected User() { }

        public User(string name, string email, string phone, UserRole role)
        {
            SetName(name);
            SetEmail(email);
            Phone = phone;
            Role = role;
            IsActive = true;
        }
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required");

            Name = name;
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required");

            UserEmail = email.ToLower();
        }

        public void SetPasswordHash(string hash)
        {
            PasswordHash = hash;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
