using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Bookings;
using DomainLayer.shops;
using DomainLayer.Enums;
using DomainLayer.Common;
using DomainLayer.Auditing;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.Bookings.Commands.AssignWarehouse
{
    public class AssignWarehouseCommandHandler : IRequestHandler<AssignWarehouseCommand, OperationResult<Unit>>
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IWarehouseRepository _warehouseRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IAuditRepository _auditRepo;

        public AssignWarehouseCommandHandler(
            IBookingRepository bookingRepo,
            IWarehouseRepository warehouseRepo,
            ICompanyRepository companyRepo,
            IBranchRepository branchRepo,
            IAuditRepository auditRepo)
        {
            _bookingRepo = bookingRepo;
            _warehouseRepo = warehouseRepo;
            _companyRepo = companyRepo;
            _branchRepo = branchRepo;
            _auditRepo = auditRepo;
        }

        public async Task<OperationResult<Unit>> Handle(AssignWarehouseCommand request, CancellationToken cancellationToken)
        {
            // Authorization: only shop manager/owner can assign
            if (request.ActorRole != UserRole.ShopManager && request.ActorRole != UserRole.ShopOwner)
                return OperationResult<Unit>.Failure("Unauthorized.");

            var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
                return OperationResult<Unit>.Failure("Booking not found.");

            var warehouse = await _warehouseRepo.GetByIdAsync(request.WarehouseId, cancellationToken);
            if (warehouse is null)
                return OperationResult<Unit>.Failure("Warehouse not found.");

            if (warehouse.BranchId != booking.BranchId)
                return OperationResult<Unit>.Failure("You can only assign warehouses from your branch.");

            if (warehouse.IsFull())
                return OperationResult<Unit>.Failure("Selected warehouse is full.");

            // Assign and update capacity
            booking.AssignWarehouse(warehouse.Id);
            warehouse.IncreaseUsage();

            // Persist changes
            await _companyRepo.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditRepo.LogAsync(request.ActorUserId,
                AuditActions.TireSetUpdated, // no storage action defined; reuse existing constant
                nameof(Booking),
                booking.Id,
                true,
                null,
                null);

            return OperationResult<Unit>.Success(Unit.Value);
        }
    }
}
