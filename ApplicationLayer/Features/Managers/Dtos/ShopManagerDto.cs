using DomainLayer.Enums;

namespace ApplicationLayer.Features.Managers.Dtos
{
    public class ShopManagerDto
    {
        public Guid UserId { get; set; }
        public Guid ShopCompanyId { get; set; }
        public Guid BranchId { get; set; }
        public string Name { get; set; } = default!;

        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
        public UserRole Role { get; set; }
    }
}
