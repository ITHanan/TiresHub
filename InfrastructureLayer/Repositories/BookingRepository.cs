using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Bookings;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Booking booking, CancellationToken ct)
    {
        await _context.Bookings.AddAsync(booking, ct);
        await _context.SaveChangesAsync(ct);
    }

    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct)
    {
        return _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
    }

    public async Task<IReadOnlyList<Booking>> GetForVehicleOwnerAsync(Guid ownerUserId, CancellationToken ct)
    {
        // Antag: Vehicle har OwnerUserId (ändra om du har annan modell)
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => _context.Vehicles.Any(v => v.Id == b.VehicleId && v.OwnerId == ownerUserId))
            .OrderBy(b => b.AppointmentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetForManagerAsync(Guid managerUserId, CancellationToken ct)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => _context.Branches.Any(br =>
                br.Id == b.BranchId &&
                br.Employees.Any(e => e.Id == managerUserId)))
            .OrderBy(b => b.AppointmentDate)
            .ToListAsync(ct);
    }
    public async Task<List<BookingSummaryDto>> GetBookingSummariesByBranchIdAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .Where(b => b.BranchId == branchId)
            .Join(_context.Vehicles,
                booking => booking.VehicleId,
                vehicle => vehicle.Id,
                (booking, vehicle) => new BookingSummaryDto
                {
                    Id = booking.Id,
                    VehiclePlateNumber = vehicle.PlateNumber,
                    ServiceType = booking.ServiceType,
                    AppointmentDate = booking.AppointmentDate,
                    Status = booking.Status,
                    BranchId = booking.BranchId
                })
            .OrderBy(b => b.AppointmentDate)
            .ToListAsync(cancellationToken);
    }
    public async Task<BookingDetailsDto?> GetBookingDetailsAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Where(b => b.Id == bookingId)
            .Join(_context.Vehicles,
                booking => booking.VehicleId,
                vehicle => vehicle.Id,
                (booking, vehicle) => new { booking, vehicle })
            .Join(_context.Branches,
                bv => bv.booking.BranchId,
                branch => branch.Id,
                (bv, branch) => new BookingDetailsDto
                {
                    Id = bv.booking.Id,
                    VehiclePlateNumber = bv.vehicle.PlateNumber,
                    ServiceType = bv.booking.ServiceType,
                    AppointmentDate = bv.booking.AppointmentDate,
                    Status = bv.booking.Status,
                    BranchId = bv.booking.BranchId,
                    BranchName = branch.Name,
                    WarehouseId = bv.booking.WarehouseId ?? Guid.Empty,
                    AssignedEmployeeId = bv.booking.AssignedEmployeeId ?? Guid.Empty
                })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
