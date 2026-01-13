using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Vehicles
{
    public class VehicleStoragePreference:BaseEntity
    {
        public Guid VehicleId { get; private set; }
        public Guid BranchId { get; private set; }
        public Guid WarehouseId { get; private set; }

        protected VehicleStoragePreference() { }

        public VehicleStoragePreference(Guid vehicleId, Guid branchId, Guid warehouseId)
        {
            VehicleId = vehicleId;
            BranchId = branchId;
            WarehouseId = warehouseId;
        }

        public void UpdateWarehouse(Guid newWarehouseId)
        {
            WarehouseId = newWarehouseId;
        }
    }
}
