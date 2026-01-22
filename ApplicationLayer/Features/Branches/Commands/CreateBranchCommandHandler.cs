using ApplicationLayer.Features.Branches.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.shops;
using MediatR;

namespace ApplicationLayer.Features.Branches.Commands.CreateBranch
{
    public class CreateBranchCommandHandler
        : IRequestHandler<CreateBranchCommand, BranchDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICompanyRepository _companyRepo;
        private readonly IBranchRepository _branchRepo;

        public CreateBranchCommandHandler(
            ICurrentUser currentUser,
            ICompanyRepository companyRepo,
            IBranchRepository branchRepo)
        {
            _currentUser = currentUser;
            _companyRepo = companyRepo;
            _branchRepo = branchRepo;
        }

        public async Task<BranchDto> Handle(
            CreateBranchCommand request,
            CancellationToken ct)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            var exists = await _branchRepo.BranchNameExistsAsync(
                request.ShopCompanyId,
                request.Name,
                ct);

            if (exists)
                throw new InvalidOperationException("Branch already exists.");

            var branch = new Branch(
                name: request.Name,
                city: request.City,
                address: request.Address,
                shopCompanyId: request.ShopCompanyId
            );

            await _branchRepo.AddAsync(branch, ct);
            await _companyRepo.SaveChangesAsync(ct);

            return new BranchDto
            {
                Id = branch.Id,
                ShopCompanyId = branch.ShopCompanyId,
                Name = branch.Name,
                City = branch.City,
                Address = branch.Address,
                IsActive = branch.IsActive
            };
        }
    }
}
