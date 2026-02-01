using DomainLayer.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfrastructureLayer.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.ToTable("Bookings");
        b.HasKey(x => x.Id);

        b.Property(x => x.ServiceType).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.TireType).HasConversion<int?>();

        b.HasIndex(x => new { x.BranchId, x.AppointmentDate });
    }
}
