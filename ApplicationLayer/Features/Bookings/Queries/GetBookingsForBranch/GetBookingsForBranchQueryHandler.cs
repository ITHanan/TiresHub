using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch
{
    public class GetBookingsForBranchQueryHandler : IRequestHandler<GetBookingsForBranchQuery, List<BookingSummaryDto>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;

        public GetBookingsForBranchQueryHandler(
            ICurrentUser currentUser,
            IBookingRepository bookingRepository,
            IUserRepository userRepository)
        {
            _currentUser = currentUser;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
        }

        public async Task<List<BookingSummaryDto>> Handle(GetBookingsForBranchQuery request, CancellationToken cancellationToken)
        {
            // Ensure user is authenticated
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            // Ensure user is a ShopManager
            if (_currentUser.Role != UserRole.ShopManager)
                throw new UnauthorizedAccessException("Only shop managers can view branch bookings.");

            // Get user to check branch assignment
            var user = await _userRepository.GetByIdAsync(_currentUser.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            if (!user.BranchId.HasValue)
                throw new UnauthorizedAccessException("Shop manager is not assigned to any branch.");

            var branchId = user.BranchId.Value;

            // Query bookings for the assigned branch
            return await _bookingRepository.GetBookingSummariesByBranchIdAsync(branchId);
        }
    }
}
