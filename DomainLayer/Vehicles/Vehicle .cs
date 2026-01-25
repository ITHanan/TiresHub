using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Vehicles
{
    public class Vehicle : BaseEntity
    {
        public string PlateNumber { get; private set; } = default!;
        public string? Make { get; private set; }
        public string? Model { get; private set; }
        public int? Year { get; private set; }

        public Guid OwnerId { get; private set; }

        public bool HasCompletedService { get; private set; }

        public ICollection<TireSet> TireSets { get; private set; } = new List<TireSet>();

        protected Vehicle() { }

        public Vehicle(string plateNumber, Guid ownerId, string? make = null, string? model = null, int? year = null)
        {
            SetPlateNumber(plateNumber);
            SetYear(year);
            Make = make;
            OwnerId = ownerId;
            Model = model;
            Year = year;
        }

        private void SetPlateNumber(string plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                throw new ArgumentException("Plate number is required");

            PlateNumber = plateNumber
                .Trim()
                .ToUpperInvariant();
        }



        private void SetYear(int? year)
        {
            if (year is null)
                return;

            if (year < 1900 || year > DateTime.UtcNow.Year + 1)
                throw new ArgumentException("Invalid vehicle year");

            Year = year;
        }


        public void MarkServiceCompleted()
        {
            HasCompletedService = true;
        }

        
    }
}