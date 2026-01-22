using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.Features.Branches.DTOs;
using MediatR;

namespace ApplicationLayer.Features.Branches.Commands.CreateBranch
{
    public record CreateBranchCommand(
        Guid ShopCompanyId,
        string Name,
        string City,
        string Address
    ) : IRequest<BranchDto>;
}
