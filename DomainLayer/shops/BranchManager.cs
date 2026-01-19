using DomainLayer.shops;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Shops
{
    public class BranchManager
    {
        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; }

        public Guid ShopManagerId { get; private set; }
        public ShopManager ShopManager { get; private set; }

        protected BranchManager() { }

        public BranchManager(Guid branchId, Guid shopManagerId)
        {
            BranchId = branchId;
            ShopManagerId = shopManagerId;
        }
    }
}

