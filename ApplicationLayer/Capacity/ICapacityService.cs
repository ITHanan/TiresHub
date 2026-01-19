using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Capacity;

public interface ICapacityService
{
    Task UpdateWarehouseCapacityAsync(Guid warehouseId, UpdateCapacityRequest request, CancellationToken ct);
}

