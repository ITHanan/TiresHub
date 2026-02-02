using DomainLayer.Common;
using DomainLayer.Enums;


namespace DomainLayer.Bookings;

public class Booking : BaseEntity
{
    public ServiceType ServiceType { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime AppointmentDate { get; private set; }

    public Guid VehicleId { get; private set; }
    public Guid BranchId { get; private set; }

    public Guid? WarehouseId { get; private set; }
    public Guid? AssignedEmployeeId { get; private set; }

    // UC-11 (service-specifik data)
    public TireType? TireType { get; private set; }   // ChangeTires
    public int? Quantity { get; private set; }        // BuyNewTires

    protected Booking() { }

    private Booking(
        ServiceType serviceType,
        DateTime appointmentDate,
        Guid vehicleId,
        Guid branchId)
    {
        if (vehicleId == Guid.Empty)
            throw new DomainException("VehicleId is required.");

        if (branchId == Guid.Empty)
            throw new DomainException("BranchId is required.");

        if (appointmentDate.Date < DateTime.UtcNow.Date)
            throw new DomainException("Appointment date must be today or later.");

        ServiceType = serviceType;
        AppointmentDate = appointmentDate;
        VehicleId = vehicleId;
        BranchId = branchId;

        Status = BookingStatus.InProgress;
    }

    // 🔹 UC-10: Factory method
    public static Booking Create(
        ServiceType serviceType,
        DateTime appointmentDate,
        Guid vehicleId,
        Guid branchId,
        TireType? tireType,
        int? quantity)
    {
        var booking = new Booking(serviceType, appointmentDate, vehicleId, branchId);
        booking.ApplyServiceRules(tireType, quantity);
        return booking;
    }

    // 🔹 UC-11: ServiceType-regler
    private void ApplyServiceRules(TireType? tireType, int? quantity)
    {
        if (ServiceType == ServiceType.ChangeTires)
        {
            if (tireType is null)
                throw new DomainException("TireType is required for ChangeTires.");

            TireType = tireType;
            Quantity = null;
            return;
        }

        if (ServiceType == ServiceType.BuyNewTires)
        {
            if (quantity is null || quantity <= 0)
                throw new DomainException("Quantity is required for BuyNewTires.");

            Quantity = quantity;
            TireType = null;
            return;
        }

        throw new DomainException("Invalid service type.");
    }

    // 🔹 UC-12
    public void Confirm() => Status = BookingStatus.Confirmed;

    public void Cancel() => Status = BookingStatus.Cancelled;
    public void Complete() => Status = BookingStatus.Completed;

    public void AssignWarehouse(Guid warehouseId) => WarehouseId = warehouseId;
    public void AssignEmployee(Guid employeeId) => AssignedEmployeeId = employeeId;
}
