using DomainLayer.Common;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.shops
{
    public class BranchManager: BaseEntity
    {
        public Guid BranchId { get;  set; }  
        public Branch Branch { get;  set; } 
        public Guid ShopManagerId { get;  set; }
        public User ShopManager { get;  set; }
    }
}
