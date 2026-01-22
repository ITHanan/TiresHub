using DomainLayer.Common;

namespace DomainLayer.shops
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public int Capacity { get; private set; }
        public int CurrentUsage { get; private set; }
        public bool IsActive { get; private set; }

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = default!;

        protected Warehouse() { }

        public Warehouse(string name, int capacity, Guid branchId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.");

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");

            Name = name;
            Capacity = capacity;
            BranchId = branchId;
            CurrentUsage = 0;
            IsActive = true;
        }

        public bool IsFull() => CurrentUsage >= Capacity;

        public void IncreaseUsage()
        {
            if (IsFull())
                throw new InvalidOperationException("Warehouse is full.");

            CurrentUsage++;
        }

        public void DecreaseUsage()
        {
            if (CurrentUsage <= 0)
                throw new InvalidOperationException("Warehouse usage is already zero.");

            CurrentUsage--;
        }
    }
}
