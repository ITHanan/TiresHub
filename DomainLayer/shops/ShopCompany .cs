using DomainLayer.Common;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.shops
{
    public class ShopCompany:BaseEntity
    {

        public string Name { get; private set; }

        public Guid OwnerId { get; private set; }
        public User Owner { get; private set; }

        public ICollection<Branch> Branches { get; private set; } = new List<Branch>();

        protected ShopCompany() { }

        public ShopCompany(string name, Guid ownerId)
        {
            Name = name;
            OwnerId = ownerId;
        }
    }
}

