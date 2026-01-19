using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DomainLayer.Shops
{
    public class ShopManager : BaseEntity
    {
        public string Name { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }

        public bool IsActive { get; private set; }

        // Branch access (many-to-many)
        public ICollection<BranchManager> BranchManagers { get; private set; } = new List<BranchManager>();

        protected ShopManager() { }

        public ShopManager(string name, string? email, string? phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Email or phone is required.");

            Name = name.Trim();
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
