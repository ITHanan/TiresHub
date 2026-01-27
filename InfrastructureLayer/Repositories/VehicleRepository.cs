using ApplicationLayer.Interfaces;
using DomainLayer.Vehicles;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;

        public VehicleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(Guid ownerId, string plateNumber)
        {
            return await _context.Vehicles.AnyAsync(v =>
                v.OwnerId == ownerId &&
                v.PlateNumber == plateNumber);
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Vehicle?> GetByIdAsync(Guid vehicleId)
        {
            return await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task<List<Vehicle>> GetByOwnerAsync(Guid ownerId)
        {
            return await _context.Vehicles
                .Where(v => v.OwnerId == ownerId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Vehicle>> GetActiveByOwnerAsync(Guid ownerId)
        {
            return await _context.Vehicles
                .Where(v => v.OwnerId == ownerId && v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

    }
}
