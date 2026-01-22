using MediatR;

namespace ApplicationLayer.Features.Warehouses.Commands
{
    public record DecreaseWarehouseUsageCommand(Guid WarehouseId) : IRequest;
}
