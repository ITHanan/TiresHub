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

    public async Task<Guid?> ReserveWarehouseAndAddBookingAsync(Booking booking, CancellationToken ct)
    {
        // Use a transaction and a conditional update to atomically reserve capacity on a single warehouse
        using var tx = await _context.Database.BeginTransactionAsync(ct);

        // Find a warehouse that has available capacity
        var warehouse = await _context.Warehouses
            .Where(w => w.BranchId == booking.BranchId && w.IsActive && w.CurrentUsage < w.Capacity)
            .OrderBy(w => w.Id) // stable ordering
            .FirstOrDefaultAsync(ct);

        if (warehouse == null)
        {
            await tx.RollbackAsync(ct);
            return null;
        }

        // Increment usage directly in DB to avoid concurrency issues
        var updated = await _context.Database.ExecuteSqlRawAsync(
            "UPDATE Warehouses SET CurrentUsage = CurrentUsage + 1 WHERE Id = {0} AND CurrentUsage < Capacity",
            new object[] { warehouse.Id }, ct);

        if (updated == 0)
        {
            // someone else took the slot concurrently
            await tx.RollbackAsync(ct);
            return null;
        }

        // Reload the warehouse entity to reflect updated CurrentUsage
        await _context.Entry(warehouse).ReloadAsync(ct);

        // Assign to booking
        booking.AssignWarehouse(warehouse.Id);

        await _context.Bookings.AddAsync(booking, ct);
        await _context.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return warehouse.Id;
    }

    public async Task<InspectionReport?> GetInspectionReportByBookingIdAsync(Guid bookingId, CancellationToken ct)
    {
        return await _context.InspectionReports
            .Include(r => r.Photos)
            .Include(r => r.CreatedByUser)
            .FirstOrDefaultAsync(r => r.BookingId == bookingId, ct);
    }

    public async Task<string?> GetOwnerDecisionByBookingIdAsync(Guid bookingId, CancellationToken ct)
    {
        var decision = await _context.OwnerDecisions
            .AsNoTracking()
            .Where(d => d.BookingId == bookingId)
            .Select(d => d.Decision.ToString())
            .FirstOrDefaultAsync(ct);

        return decision;
    }
}
