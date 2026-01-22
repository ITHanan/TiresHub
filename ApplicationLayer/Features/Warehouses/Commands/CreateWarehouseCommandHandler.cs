
using ApplicationLayer.Features.Warehouses.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.shops;
using MediatR;

namespace ApplicationLayer.Features.Warehouses.Commands.CreateWarehouse
{
    public class CreateWarehouseCommandHandler
        : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
    {
        private readonly IBranchRepository _branchRepo;
        private readonly IWarehouseRepository _warehouseRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly ICurrentUser _currentUser;

        public CreateWarehouseCommandHandler(
            IBranchRepository branchRepo,
            IWarehouseRepository warehouseRepo,
            ICompanyRepository companyRepo,
            ICurrentUser currentUser)
        {
            _branchRepo = branchRepo;
            _warehouseRepo = warehouseRepo;
            _companyRepo = companyRepo;
            _currentUser = currentUser;
        }

        public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken ct)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            var branchExists = await _branchRepo.BranchExistsAsync(request.BranchId, ct);
            if (!branchExists)
                throw new InvalidOperationException("Branch not found.");

            var companyId = await _branchRepo.GetCompanyIdByBranchIdAsync(request.BranchId, ct);
            if (companyId is null)
                throw new InvalidOperationException("Branch not found.");

            var owned = await _companyRepo.OwnedByAsync(companyId.Value, _currentUser.UserId, ct);
            if (!owned)
                throw new UnauthorizedAccessException("You do not have permission to manage this branch.");

            var exists = await _warehouseRepo.ExistsAsync(request.BranchId, request.Name, ct);
            if (exists)
                throw new InvalidOperationException("Warehouse already exists in this branch.");

            var warehouse = new Warehouse(
                name: request.Name,
                capacity: request.Capacity,
                branchId: request.BranchId
            );

            await _warehouseRepo.AddAsync(warehouse, ct);
            await _companyRepo.SaveChangesAsync(ct);

            return new WarehouseDto
            {
                Id = warehouse.Id,
                BranchId = warehouse.BranchId,
                Name = warehouse.Name,
                Capacity = warehouse.Capacity,
                CurrentUsage = warehouse.CurrentUsage,
                IsActive = warehouse.IsActive
            };
        }
    }
}

