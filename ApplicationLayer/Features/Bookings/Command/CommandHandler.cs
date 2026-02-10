using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Availability;
using ApplicationLayer.Interfaces.Bookings;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Bookings;
using DomainLayer.Enums;
using MediatR;
using System.Linq;

namespace ApplicationLayer.Features.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly ICurrentUser _currentUser;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IBranchAvailabilityRepository _availability;
    private readonly IBookingRepository _bookingRepo;

    public CreateBookingCommandHandler(
        ICurrentUser currentUser,
        IVehicleRepository vehicleRepo,
        IBranchAvailabilityRepository availability,
        IBookingRepository bookingRepo)
    {
        _currentUser = currentUser;
        _vehicleRepo = vehicleRepo;
        _availability = availability;
        _bookingRepo = bookingRepo;
    }

    public async Task<Guid> Handle(CreateBookingCommand cmd, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        if (_currentUser.Role != UserRole.VehicleOwner)
            throw new UnauthorizedAccessException();

        var r = cmd.Request;

        var ownsVehicle = await _vehicleRepo
            .IsOwnedByUserAsync(r.VehicleId, _currentUser.UserId, ct);

        if (!ownsVehicle)
            throw new UnauthorizedAccessException("Vehicle does not belong to user.");

        // UC-10: branch capacity check
        await _availability.EnsureBranchHasCapacityAsync(r.BranchId, ct);

        var booking = Booking.Create(
            r.ServiceType,
            r.AppointmentDate,
            r.VehicleId,
            r.BranchId,
            r.TireType,
            r.Quantity);

        booking.Confirm();

        // Atomically reserve warehouse and persist booking using repository
        var reservedWarehouseId = await _bookingRepo.ReserveWarehouseAndAddBookingAsync(booking, ct);

        if (reservedWarehouseId == null)
            throw new InvalidOperationException("Selected branch is currently unavailable for new bookings.");

        return booking.Id;
    }
}
