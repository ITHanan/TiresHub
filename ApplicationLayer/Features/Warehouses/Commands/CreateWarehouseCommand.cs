using ApplicationLayer.Features.Warehouses.DTOs;
using MediatR;

namespace ApplicationLayer.Features.Warehouses.Commands.CreateWarehouse
{
    public record CreateWarehouseCommand(
        Guid BranchId,
        string Name,
        int Capacity
    ) : IRequest<WarehouseDto>;
}
