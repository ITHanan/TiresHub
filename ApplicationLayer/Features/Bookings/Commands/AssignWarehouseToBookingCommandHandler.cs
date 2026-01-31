using ApplicationLayer.Features.Bookings.Dtos;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Commands.AssignWarehouse
{
    public class AssignWarehouseToBookingCommandHandler
        : IRequestHandler<AssignWarehouseToBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IWarehouseRepository _warehouseRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICurrentUser _currentUser;

        public AssignWarehouseToBookingCommandHandler(
            IBookingRepository bookingRepo,
            IWarehouseRepository warehouseRepo,
            IBranchRepository branchRepo,
            ICompanyRepository companyRepo,
            IUserRepository userRepo,
            ICurrentUser currentUser)
        {
            _bookingRepo = bookingRepo;
            _warehouseRepo = warehouseRepo;
            _branchRepo = branchRepo;
            _companyRepo = companyRepo;
            _userRepo = userRepo;
            _currentUser = currentUser;
        }

        public async Task<BookingDto> Handle(AssignWarehouseToBookingCommand request, CancellationToken ct)
        {
            // 1. Authentication check
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Not authenticated.");

            // 2. Role check - only ShopManager can assign warehouse
            if (_currentUser.Role != UserRole.ShopManager)
                throw new UnauthorizedAccessException("Only shop managers can assign storage locations.");

            // 3. Get and validate booking exists
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId, ct);
            if (booking is null)
                throw new InvalidOperationException("Booking not found.");

            // 4. Validate booking is in Confirmed status
            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException("Only confirmed bookings can be assigned to warehouse.");

            // 5. Get and validate warehouse exists
            var warehouse = await _warehouseRepo.GetByIdAsync(request.WarehouseId, ct);
            if (warehouse is null)
                throw new InvalidOperationException("Warehouse not found.");

            // 6. Validate warehouse is not full
            if (warehouse.IsFull())
                throw new InvalidOperationException("Warehouse is full and cannot accept more bookings.");

            // 7. Validate warehouse belongs to the same branch as booking (prevent cross-branch assignment)
            if (warehouse.BranchId != booking.BranchId)
                throw new UnauthorizedAccessException("Cannot assign warehouse from a different branch.");

            // 8. Get current user details to validate branch assignment
            var currentUserDetails = await _userRepo.GetByIdAsync(_currentUser.UserId, ct);
            if (currentUserDetails is null)
                throw new UnauthorizedAccessException("User not found.");

            // 9. Validate manager is assigned to the branch
            if (currentUserDetails.BranchId != booking.BranchId)
                throw new UnauthorizedAccessException("You can only assign warehouses for bookings in your assigned branch.");

            // 10. Assign warehouse to booking
            booking.AssignWarehouse(request.WarehouseId);

            // 11. Increase warehouse usage
            warehouse.IncreaseUsage();

            // 12. Persist changes
            await _companyRepo.SaveChangesAsync(ct);

            // 13. Return DTO
            return new BookingDto
            {
                Id = booking.Id,
                ServiceType = booking.ServiceType,
                Status = booking.Status,
                AppointmentDate = booking.AppointmentDate,
                VehicleId = booking.VehicleId,
                BranchId = booking.BranchId,
                WarehouseId = booking.WarehouseId,
                AssignedEmployeeId = booking.AssignedEmployeeId
            };
        }
    }
}
