using DomainLayer.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IVehicleRepository
    {
        Task<bool> ExistsAsync(Guid ownerId, string plateNumber);
        Task AddAsync(DomainLayer.Vehicles.Vehicle vehicle);

        Task<Vehicle?> GetByIdAsync(Guid vehicleId);

        Task<List<Vehicle>> GetByOwnerAsync(Guid ownerId);

        Task<List<Vehicle>> GetActiveByOwnerAsync(Guid ownerId);
        Task SaveChangesAsync();
    }
}
