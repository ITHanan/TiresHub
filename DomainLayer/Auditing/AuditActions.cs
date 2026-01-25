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

        // ================= TIRE SETS =================
        public const string TireSetCreated = "TireSetCreated";
        public const string TireSetCreateFailed = "TireSetCreateFailed";

        public const string TireSetUpdated = "TireSetUpdated";
        public const string TireSetUpdateFailed = "TireSetUpdateFailed";

        public const string TireSetLocked = "TireSetLocked";

    
    }
}
