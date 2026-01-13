using DomainLayer.Common;

namespace DomainLayer.shops
{
    public class Warehouse: BaseEntity
    {
        public string Name { get; private set; }
        public int Capacity { get; private set; }
        public int CurrentUsage { get; private set; }
        public bool IsActive { get; private set; }

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; }

        protected Warehouse() { }

        public Warehouse(string name, int capacity, Guid branchId)
        {
            Name = name;
            Capacity = capacity;
            BranchId = branchId;
            IsActive = true;
        }

        public bool IsFull() => CurrentUsage >= Capacity;

        public void IncreaseUsage() => CurrentUsage++;
        public void DecreaseUsage() => CurrentUsage--;
    }
}
