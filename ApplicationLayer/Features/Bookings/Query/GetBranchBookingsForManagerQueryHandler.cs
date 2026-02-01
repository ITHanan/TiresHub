using ApplicationLayer.Features.Bookings.DTOs;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Bookings;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Enums;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetBranchBookingsForManager;

public sealed class GetBranchBookingsForManagerQueryHandler
    : IRequestHandler<GetBranchBookingsForManagerQuery, IReadOnlyList<BookingListItemDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IBookingRepository _bookingRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IBranchRepository _branchRepo;

    public GetBranchBookingsForManagerQueryHandler(
        ICurrentUser currentUser,
        IBookingRepository bookingRepo,
        IVehicleRepository vehicleRepo,
        IBranchRepository branchRepo)
    {
        _currentUser = currentUser;
        _bookingRepo = bookingRepo;
        _vehicleRepo = vehicleRepo;
        _branchRepo = branchRepo;
    }

    public async Task<IReadOnlyList<BookingListItemDto>> Handle(GetBranchBookingsForManagerQuery request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated) throw new UnauthorizedAccessException();
        if (_currentUser.Role != UserRole.ShopManager) throw new UnauthorizedAccessException();

        var bookings = await _bookingRepo.GetForManagerAsync(_currentUser.UserId, ct);

        var result = new List<BookingListItemDto>(bookings.Count);
        foreach (var b in bookings.OrderBy(x => x.AppointmentDate))
        {
            var plate = await _vehicleRepo.GetPlateNumberAsync(b.VehicleId, ct) ?? "";
            var branchName = await _branchRepo.GetBranchNameAsync(b.BranchId, ct) ?? "";

            result.Add(new BookingListItemDto
            {
                BookingId = b.Id,
                AppointmentDate = b.AppointmentDate,
                ServiceType = b.ServiceType,
                Status = b.Status,
                VehiclePlateNumber = plate,
                BranchName = branchName
            });
        }
        return result;
    }
}
