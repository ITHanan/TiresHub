namespace ApplicationLayer.Features.Branches.DTOs
{
    public class BranchDto
    {
        public Guid Id { get; set; }
        public Guid ShopCompanyId { get; set; }
        public string Name { get; set; } = default!;
        public string City { get; set; } = default!;
        public string Address { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}

