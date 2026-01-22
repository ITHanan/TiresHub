using ApplicationLayer.Features.Warehouses.DTOs;
using MediatR;

namespace ApplicationLayer.Warehouses.Queries.GetWarehouses
{
    public record GetWarehousesByBranchQuery(Guid BranchId) : IRequest<List<WarehouseDto>>;
}
