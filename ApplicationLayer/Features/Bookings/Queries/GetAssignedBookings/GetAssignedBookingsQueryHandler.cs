using ApplicationLayer.Interfaces.Bookings;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetAssignedBookings;

public sealed class GetAssignedBookingsQueryHandler : IRequestHandler<GetAssignedBookingsQuery, List<AssignedBookingDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IBookingRepository _bookingRepo;

    public GetAssignedBookingsQueryHandler(ICurrentUser currentUser, IBookingRepository bookingRepo)
    {
        _currentUser = currentUser;
        _bookingRepo = bookingRepo;
    }

    public async Task<List<AssignedBookingDto>> Handle(GetAssignedBookingsQuery request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User not authenticated.");

        if (_currentUser.Role != UserRole.Employee)
            throw new UnauthorizedAccessException("Only employees can access assigned bookings.");

        var result = await _bookingRepo.GetAssignedBookingsAsync(_currentUser.UserId, ct);

        // result is already ordered in repository by AppointmentDate ascending
        return result;
    }
}