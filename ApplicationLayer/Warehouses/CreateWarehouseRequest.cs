using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Warehouses;

public record CreateWarehouseRequest(Guid BranchId, string Name);
public record UpdateWarehouseRequest(string Name, bool IsActive);

