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
        public Guid VehicleId { get; private set; }
        public TireType TireType { get; private set; }
        public string Size { get; private set; } = default!;
        public string Brand { get; private set; } = default!;
        public string? Notes { get; private set; }
        public bool IsLocked { get; private set; }

        protected TireSet() { }

        public TireSet(Guid vehicleId, TireType tireType, string size, string brand, string? notes = null)
        {
            VehicleId = vehicleId;
            TireType = tireType;

            SetSize(size);
            SetBrand(brand);

            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            IsLocked = false;
        }

        public void Update(string size, string brand, string? notes)
        {
            if (string.IsNullOrWhiteSpace(size))
                throw new ArgumentException("Tire size is required.");

            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Tire brand is required.");

            Size = size.Trim();
            Brand = brand.Trim();
            Notes = notes;
        }

        public void Lock()
        {
            if (IsLocked) return;
            IsLocked = true;
        }

        public void UpdateByManager(string size, string brand, string? notes)
        {
            // Post-service updates (for the manager later)
            SetSize(size);
            SetBrand(brand);
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        private void SetSize(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
                throw new ArgumentException("Tire size is required.");

            Size = size.Trim();
        }

        private void SetBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Tire brand is required.");

            Brand = brand.Trim();
        }
    }
}
