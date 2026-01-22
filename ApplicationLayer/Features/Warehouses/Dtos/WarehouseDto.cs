namespace ApplicationLayer.Features.Warehouses.DTOs
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string Name { get; set; } = default!;
        public int Capacity { get; set; }
        public int CurrentUsage { get; set; }
        public bool IsActive { get; set; }
    }
}
