using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Get booking details first (we need AssignedEmployeeId/BranchId for checks)
            var bookingDetails = await _bookingRepository.GetBookingDetailsAsync(request.BookingId, cancellationToken);

            if (bookingDetails == null)
                throw new InvalidOperationException("Booking not found.");

            // If shop manager -> verify branch ownership
            if (_currentUser.Role == UserRole.ShopManager)
            {
                // Get user to check branch assignment
                var user = await _userRepository.GetByIdAsync(_currentUser.UserId);
                if (user == null)
                    throw new UnauthorizedAccessException("User not found.");

                if (!user.BranchId.HasValue)
                    throw new UnauthorizedAccessException("Shop manager is not assigned to any branch.");

                var branchId = user.BranchId.Value;

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

            // If employee -> allow only if assigned to them
            if (_currentUser.Role == UserRole.Employee)
            {
                var assigned = bookingDetails.AssignedEmployeeId;
                if (assigned != _currentUser.UserId)
                {
                    // Log unauthorized attempt
                    await _auditRepository.LogAsync(
                        userId: _currentUser.UserId,
                        action: "UnauthorizedBookingAccessByEmployee",
                        entityType: "Booking",
                        entityId: request.BookingId,
                        success: false,
                        reason: $"Employee {_currentUser.UserId} attempted to access booking assigned to {assigned}");

                    throw new UnauthorizedAccessException("You do not have access to this booking.");
                }

                return bookingDetails;
            }

            // Other roles not allowed
            throw new UnauthorizedAccessException("Only shop managers or the assigned employee can view booking details.");
        }
    }
}
