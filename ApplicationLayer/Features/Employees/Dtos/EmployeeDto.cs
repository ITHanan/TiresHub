using DomainLayer.Enums;

namespace ApplicationLayer.Features.Employees.Dtos
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public bool IsActive { get; set; }
        public UserRole Role { get; set; }
    }
}
