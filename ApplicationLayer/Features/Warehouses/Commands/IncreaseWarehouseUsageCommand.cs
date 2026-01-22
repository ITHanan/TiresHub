using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace ApplicationLayer.Features.Warehouses.Commands.Usage
{
    public record IncreaseWarehouseUsageCommand(Guid WarehouseId) : IRequest;
}
