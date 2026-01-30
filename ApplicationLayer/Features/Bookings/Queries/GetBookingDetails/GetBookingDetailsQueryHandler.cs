using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingDetails
{
    public class GetBookingDetailsQueryHandler : IRequestHandler<GetBookingDetailsQuery, BookingDetailsDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditRepository _auditRepository;

        public GetBookingDetailsQueryHandler(
            ICurrentUser currentUser,
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IAuditRepository auditRepository)
        {
            _currentUser = currentUser;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _auditRepository = auditRepository;
        }

        public async Task<BookingDetailsDto> Handle(GetBookingDetailsQuery request, CancellationToken cancellationToken)
        {
            // Ensure user is authenticated
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            // Ensure user is a ShopManager
            if (_currentUser.Role != UserRole.ShopManager)
                throw new UnauthorizedAccessException("Only shop managers can view booking details.");

            // Get user to check branch assignment
            var user = await _userRepository.GetByIdAsync(_currentUser.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            if (!user.BranchId.HasValue)
                throw new UnauthorizedAccessException("Shop manager is not assigned to any branch.");

            var branchId = user.BranchId.Value;

            // Get the booking details
            var bookingDetails = await _bookingRepository.GetBookingDetailsAsync(request.BookingId);

            if (bookingDetails == null)
                throw new InvalidOperationException("Booking not found.");

            // Verify booking belongs to manager's branch
            if (bookingDetails.BranchId != branchId)
            {
                // Log unauthorized access attempt
                await _auditRepository.LogAsync(
                    userId: _currentUser.UserId,
                    action: "UnauthorizedBookingAccess",
                    entityType: "Booking",
                    entityId: request.BookingId,
                    success: false,
                    reason: $"Shop manager attempted to access booking from branch {bookingDetails.BranchId} while assigned to branch {branchId}");

                throw new UnauthorizedAccessException("You do not have access to this booking. It belongs to another branch.");
            }

            return bookingDetails;
        }
    }
}
