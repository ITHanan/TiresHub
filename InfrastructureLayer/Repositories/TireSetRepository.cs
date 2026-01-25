using ApplicationLayer.Interfaces;
using DomainLayer.Enums;
using DomainLayer.Vehicles;
using InfrastructureLayer.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repositories
{
    public class TireSetRepository : ITireSetRepository
    {
        private readonly AppDbContext _context;

        public TireSetRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<bool> ExistsAsync(Guid vehicleId, TireType tireType)
        {
            return await _context.TireSets.AnyAsync(t =>
                t.VehicleId == vehicleId &&
                (TireType)t.TireType == tireType);
        }

        public async Task AddAsync(TireSet tireSet)
        {
            await _context.TireSets.AddAsync(tireSet);
        }

        public async Task<List<TireSet>> ListByVehicleAsync(Guid vehicleId)
        {
            return await _context.TireSets
                .Where(t => t.VehicleId == vehicleId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TireSet?> GetByIdAsync(Guid tireSetId)
        {
            return await _context.TireSets.FirstOrDefaultAsync(t => t.Id == tireSetId);
        
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
