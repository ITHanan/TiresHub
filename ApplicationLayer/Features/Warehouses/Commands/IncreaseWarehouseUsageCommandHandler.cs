using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using MediatR;

namespace ApplicationLayer.Features.Warehouses.Commands.Usage
{
    public class IncreaseWarehouseUsageCommandHandler : IRequestHandler<IncreaseWarehouseUsageCommand>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICompanyRepository _companyRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IWarehouseRepository _warehouseRepo;

        public IncreaseWarehouseUsageCommandHandler(
            ICurrentUser currentUser,
            ICompanyRepository companyRepo,
            IBranchRepository branchRepo,
            IWarehouseRepository warehouseRepo)
        {
            _currentUser = currentUser;
            _companyRepo = companyRepo;
            _branchRepo = branchRepo;
            _warehouseRepo = warehouseRepo;
        }

        public async Task Handle(IncreaseWarehouseUsageCommand request, CancellationToken ct)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            var warehouse = await _warehouseRepo.GetByIdAsync(request.WarehouseId, ct);
            if (warehouse is null)
                throw new InvalidOperationException("Warehouse not found.");

            var companyId = await _branchRepo.GetCompanyIdByBranchIdAsync(warehouse.BranchId, ct);
            if (companyId is null)
                throw new InvalidOperationException("Branch not found.");

            var owned = await _companyRepo.OwnedByAsync(companyId.Value, _currentUser.UserId, ct);
            if (!owned)
                throw new UnauthorizedAccessException("You do not have permission to manage this warehouse.");

            // Domain-regel (kastar om full)
            warehouse.IncreaseUsage();

            await _companyRepo.SaveChangesAsync(ct);
        }
    }
}
