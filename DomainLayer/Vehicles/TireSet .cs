using DomainLayer.Common;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Vehicles
{
    public class TireSet: BaseEntity    
    {
        public TireType TireType { get; private set; }
        public string Brand { get; private set; }
        public string Size { get; private set; }
        public string Model { get; private set; }
        public TireCondition Condition { get; private set; }
        public bool IsLocked { get; private set; }

        public Guid VehicleId { get; private set; }

        protected TireSet() { }

        public TireSet(TireType tireType, string brand, string size, string model, Guid vehicleId)
        {
            TireType = tireType;
            Brand = brand;
            Size = size;
            Model = model;
            Condition = TireCondition.Good;
            VehicleId = vehicleId;
        }

        public void Lock() => IsLocked = true;
    }
}
