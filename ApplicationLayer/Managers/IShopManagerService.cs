using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Managers;

public interface IShopManagerService
{
    Task<Guid> CreateAsync(CreateShopManagerRequest request, CancellationToken ct);
    Task SetActiveAsync(Guid managerId, bool isActive, CancellationToken ct);
}
