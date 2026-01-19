using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationLayer.Warehouses;

public interface IWarehouseService
{
    Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken ct);
    Task UpdateAsync(Guid warehouseId, UpdateWarehouseRequest request, CancellationToken ct);
}
