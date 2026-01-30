using ApplicationLayer.Interfaces;
using ApplicationLayer.Features.Bookings.Queries.GetBookingsForBranch.DTOs;
using ApplicationLayer.Features.Bookings.Queries.GetBookingDetails.DTOs;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<BookingSummaryDto>> GetBookingSummariesByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .Where(b => b.BranchId == branchId)
                .Join(_db.Vehicles,
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
            return await _db.Bookings
                .Where(b => b.Id == bookingId)
                .Join(_db.Vehicles,
                    booking => booking.VehicleId,
                    vehicle => vehicle.Id,
                    (booking, vehicle) => new { booking, vehicle })
                .Join(_db.Branches,
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
                        WarehouseId = bv.booking.WarehouseId,
                        AssignedEmployeeId = bv.booking.AssignedEmployeeId
                    })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
