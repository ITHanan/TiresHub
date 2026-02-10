using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Auditing;
using DomainLayer.Common;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Behaviors
{
    // This pipeline ensures that AssignWarehouseCommand cannot assign warehouse from different branch
    public class ValidateBranchOwnershipBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IWarehouseRepository _warehouseRepo;
        private readonly IAuditRepository _auditRepo;

        public ValidateBranchOwnershipBehavior(
            IBookingRepository bookingRepo,
            IWarehouseRepository warehouseRepo,
            IAuditRepository auditRepo)
        {
            _bookingRepo = bookingRepo;
            _warehouseRepo = warehouseRepo;
            _auditRepo = auditRepo;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Only apply to AssignWarehouseCommand
            if (request is ApplicationLayer.Features.Bookings.Commands.AssignWarehouse.AssignWarehouseCommand cmd)
            {
                // load booking and warehouse
                var booking = await _bookingRepo.GetByIdAsync(cmd.BookingId, cancellationToken);
                var warehouse = await _warehouseRepo.GetByIdAsync(cmd.WarehouseId, cancellationToken);

                if (booking == null || warehouse == null)
                {
                    // let handler handle not found scenarios
                    return await next();
                }

                if (booking.BranchId != warehouse.BranchId)
                {
                    // log unauthorized attempt
                    await _auditRepo.LogAsync(cmd.ActorUserId, AuditActions.UnauthorizedStorageAssignment, nameof(booking), cmd.BookingId, false, "Cross-branch assignment attempt", null);
                    // return failure OperationResult
                    var fail = DomainLayer.Common.OperationResult<Unit>.Failure("You can only assign warehouses from your branch.");

                    // must cast to TResponse - this will work when TResponse is OperationResult<Unit>
                    return (TResponse)(object)fail;
                }
            }

            return await next();
        }
    }
}
