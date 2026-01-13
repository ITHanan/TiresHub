using DomainLayer.Users;
using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.shops
{
    public class Branch:BaseEntity
    {
        public string Name { get; private set; }
        public string City { get; private set; }
        public string Address { get; private set; }
        public bool IsActive { get; private set; }

        public Guid ShopCompanyId { get; private set; }
        public ShopCompany ShopCompany { get; private set; }

        public ICollection<Warehouse> Warehouses { get; private set; } = new List<Warehouse>();
        public ICollection<User> Employees { get; private set; } = new List<User>();

        protected Branch() { }

        public Branch(string name, string city, string address, Guid shopCompanyId)
        {
            Name = name;
            City = city;
            Address = address;
            ShopCompanyId = shopCompanyId;
            IsActive = true;
        }

        public void Disable() => IsActive = false;
    }
}
