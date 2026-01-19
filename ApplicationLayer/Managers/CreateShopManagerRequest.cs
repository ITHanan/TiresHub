using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationLayer.Managers;

public record CreateShopManagerRequest(
    string Name,
    string? Email,
    string? Phone,
    List<Guid> BranchIds
);

public record DeactivateShopManagerRequest(bool IsActive);
