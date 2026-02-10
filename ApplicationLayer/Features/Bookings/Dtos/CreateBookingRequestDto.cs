using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.DTOs;

public class CreateBookingRequestDto
{
    public Guid VehicleId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime AppointmentDate { get; init; }

    public ServiceType ServiceType { get; init; }

    // UC-11
    public TireType TireType { get; init; }
    public int? Quantity { get; init; }
}
