using DomainLayer.Common;
using DomainLayer.Enums;
using DomainLayer.shops;

namespace DomainLayer.Users
{
    public class User : BaseEntity
    {
        private string email;
        private UserRole role;

        public  string  Name { get; private set; }= default!;
        public string UserEmail { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string? Phone { get; private set; }
        public UserRole Role { get; private set; }
        public bool OnboardingCompleted { get; private set; }
        public bool IsActive { get; private set; }
        public Guid? BranchId { get; private set; }

        public ICollection<BranchManager> ManagedBranches { get; set; }
       = new List<BranchManager>();





        protected User() { }

      
        public User(string name, string email, string? phone, UserRole role)
        {
            SetName(name);
            SetEmail(email);
            Phone = phone;
            Role = role;
            BranchId = null;
            IsActive = true;
            OnboardingCompleted = false;
        }

        public User(string email, UserRole role)
        {
            this.email = email;
            this.role = role;
        }


    public void AssignBranch(Guid branchId)
        {
            BranchId = branchId;
        }

        public void CompleteOnboarding()
        {
            if (OnboardingCompleted)
                return;

            OnboardingCompleted = true;
        }

        public void SetPasswordHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Password hash is required");

            PasswordHash = hash;
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



        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
