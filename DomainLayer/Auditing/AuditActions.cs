using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Auditing
{
    public static class AuditActions
    {
        public const string VehicleCreated = "VehicleCreated";
        public const string VehicleCreateFailed = "VehicleCreateFailed";

        public const string VehicleActivated = "VehicleActivated";
        public const string VehicleActivateFailed = "VehicleActivateFailed";

        public const string VehicleDeactivated = "VehicleDeactivated";
        public const string VehicleDeactivationFailed = "VehicleDeactivationFailed";

        public const string VehicleUpdated = "VehicleUpdated";
        public const string VehicleUpdateFailed = "VehicleUpdateFailed";

        // ================= TIRE SETS =================
        public const string TireSetCreated = "TireSetCreated";
        public const string TireSetCreateFailed = "TireSetCreateFailed";

        public const string TireSetUpdated = "TireSetUpdated";
        public const string TireSetUpdateFailed = "TireSetUpdateFailed";

        public const string TireSetLocked = "TireSetLocked";

        // ================= WAREHOUSE / STORAGE =================
        public const string WarehouseUsageIncreased = "WarehouseUsageIncreased";
        public const string WarehouseUsageDecreased = "WarehouseUsageDecreased";

        public const string StorageAssigned = "StorageAssigned";
        public const string UnauthorizedStorageAssignment = "UnauthorizedStorageAssignment";
    }
}
