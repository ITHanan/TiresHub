using DomainLayer.Enums;
using DomainLayer.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface ITireSetRepository
    {

        Task<bool> ExistsAsync(Guid vehicleId, TireType tireType);
        Task AddAsync(TireSet tireSet);
        Task<List<TireSet>> ListByVehicleAsync(Guid vehicleId);

        Task<TireSet?> GetByIdAsync(Guid tireSetId);
        Task SaveChangesAsync();

    }
}
