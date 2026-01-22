using ApplicationLayer.Features.Managers.Dtos;

using MediatR;

namespace ApplicationLayer.Managers.Commands.RegisterShopManager
{
    public record RegisterShopManagerCommand(
    string Name,
    string? Email,
    string? Phone,
    List<Guid> BranchId
) : IRequest<ShopManagerDto>;

}
