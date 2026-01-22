using ApplicationLayer.Features.Warehouses.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using MediatR;

namespace ApplicationLayer.Warehouses.Queries.GetWarehouses;
    public class GetWarehousesByBranchQueryHandler
        : IRequestHandler<GetWarehousesByBranchQuery, List<WarehouseDto>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICompanyRepository _companyRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IWarehouseRepository _warehouseRepo;

        public GetWarehousesByBranchQueryHandler(
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

        public async Task<List<WarehouseDto>> Handle(GetWarehousesByBranchQuery request, CancellationToken ct)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            // ownership: branch -> company -> owner
            var companyId = await _branchRepo.GetCompanyIdByBranchIdAsync(request.BranchId, ct);
            if (companyId is null)
                throw new InvalidOperationException("Branch not found.");

            var owned = await _companyRepo.OwnedByAsync(companyId.Value, _currentUser.UserId, ct);
            if (!owned)
                throw new UnauthorizedAccessException("You do not have permission to view these warehouses.");

            var warehouses = await _warehouseRepo.ListByBranchIdAsync(request.BranchId, ct);

            return warehouses.Select(w => new WarehouseDto
            {
                Id = w.Id,
                BranchId = w.BranchId,
                Name = w.Name,
                Capacity = w.Capacity,
                CurrentUsage = w.CurrentUsage,
                IsActive = w.IsActive
            }).ToList();
        }
    }

