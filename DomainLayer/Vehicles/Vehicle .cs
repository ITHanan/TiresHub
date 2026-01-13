using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Vehicles
{
    public class Vehicle: BaseEntity
    {
        public string PlateNumber { get; private set; }
        public string? Make { get; private set; }
        public string? Model { get; private set; }
        public int? Year { get; private set; }

        public Guid OwnerId { get; private set; }

        public ICollection<TireSet> TireSets { get; private set; } = new List<TireSet>();

        protected Vehicle() { }

        public Vehicle(string plateNumber, Guid ownerId, string? make = null, string? model = null, int? year = null)
        {
            PlateNumber = plateNumber;
            OwnerId = ownerId;
            Make = make;
            Model = model;
            Year = year;
        }
    }
}
