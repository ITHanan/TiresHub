using DomainLayer.Auditing;
using DomainLayer.Bookings;
using DomainLayer.Communication;
using DomainLayer.shops;
using DomainLayer.Users;
using DomainLayer.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ===================== USERS =====================
        public DbSet<User> Users => Set<User>();

        // ===================== SHOPS =====================
        public DbSet<ShopCompany> ShopCompanies => Set<ShopCompany>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();

        // ===================== VEHICLES =====================
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<TireSet> TireSets => Set<TireSet>();
        public DbSet<VehicleStoragePreference> VehicleStoragePreferences => Set<VehicleStoragePreference>();

        // ===================== BOOKINGS & INSPECTIONS =====================
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Inspection> Inspections => Set<Inspection>();
        public DbSet<InspectionReport> InspectionReports => Set<InspectionReport>();
        public DbSet<InspectionPhoto> InspectionPhotos => Set<InspectionPhoto>();

        // ===================== COMMUNICATION =====================
        public DbSet<OwnerDecision> OwnerDecisions => Set<OwnerDecision>();
        public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();

        // ===================== AUDIT =====================
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== USER =====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(u => u.UserEmail)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.HasIndex(u => u.UserEmail)
                      .IsUnique();

                entity.Property(u => u.PasswordHash)
                      .IsRequired();

                entity.Property(u => u.Role)
                      .IsRequired();
            });

            // ===================== SHOP COMPANY =====================
            modelBuilder.Entity<ShopCompany>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasMany(s => s.Branches)
                      .WithOne(b => b.ShopCompany)
                      .HasForeignKey(b => b.ShopCompanyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== BRANCH =====================
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Name)
                      .IsRequired();

                entity.Property(b => b.City)
                      .IsRequired();

                entity.Property(b => b.Address)
                      .IsRequired();
            });

            // ===================== WAREHOUSE =====================
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.Property(w => w.Name)
                      .IsRequired();

                entity.Property(w => w.Capacity)
                      .IsRequired();

                entity.HasOne(w => w.Branch)
                      .WithMany(b => b.Warehouses)
                      .HasForeignKey(w => w.BranchId);
            });

            // ===================== VEHICLE =====================
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.Property(v => v.PlateNumber)
                      .IsRequired();

                entity.HasIndex(v => v.PlateNumber);

                entity.HasMany(v => v.TireSets)
                      .WithOne()
                      .HasForeignKey(t => t.VehicleId);
            });

            // ===================== TIRE SET =====================
            modelBuilder.Entity<TireSet>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Brand)
                      .IsRequired();

                entity.Property(t => t.Size)
                      .IsRequired();

                entity.Property(t => t.TireType)
                      .IsRequired();
            });

            // ===================== VEHICLE STORAGE PREF =====================
            modelBuilder.Entity<VehicleStoragePreference>(entity =>
            {
                entity.HasKey(vs => vs.Id);

                entity.HasIndex(vs => new { vs.VehicleId, vs.BranchId })
                      .IsUnique();
            });

            // ===================== BOOKING =====================
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.ServiceType)
                      .IsRequired();

                entity.Property(b => b.Status)
                      .IsRequired();

                entity.HasOne<Vehicle>()
                      .WithMany()
                      .HasForeignKey(b => b.VehicleId);

                entity.HasOne<Branch>()
                      .WithMany()
                      .HasForeignKey(b => b.BranchId);
            });

            // ===================== INSPECTION =====================
            modelBuilder.Entity<Inspection>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.HasOne<Booking>()
                      .WithOne()
                      .HasForeignKey<Inspection>(i => i.BookingId);
            });

            // ===================== INSPECTION REPORT =====================
            modelBuilder.Entity<InspectionReport>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasMany(r => r.Photos)
                      .WithOne()
                      .HasForeignKey(p => p.InspectionReportId);
            });

            // ===================== AUDIT LOG =====================
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Action).IsRequired();
            });
        }
    }
}
