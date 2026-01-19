using DomainLayer.Auditing;
using DomainLayer.Bookings;
using DomainLayer.Communication;
using DomainLayer.shops;
using DomainLayer.Shops;
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

        // NEW (UC-07)
        public DbSet<ShopManager> ShopManagers => Set<ShopManager>();
        public DbSet<BranchManager> BranchManagers => Set<BranchManager>();

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

                // OPTIONAL: om du vill koppla Owner navigation i EF
                // entity.HasOne(s => s.Owner)
                //       .WithMany()
                //       .HasForeignKey(s => s.OwnerId)
                //       .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== BRANCH =====================
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Name).IsRequired();
                entity.Property(b => b.City).IsRequired();
                entity.Property(b => b.Address).IsRequired();

                // Optional: default value
                entity.Property(b => b.IsActive)
                      .HasDefaultValue(true);

                // NEW: Branch -> Warehouses relation is already in Warehouse config,
                // men du kan ha den här också om du vill:
                // entity.HasMany(b => b.Warehouses)
                //       .WithOne(w => w.Branch)
                //       .HasForeignKey(w => w.BranchId);
            });

            // ===================== WAREHOUSE =====================
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.Property(w => w.Name).IsRequired();

                entity.Property(w => w.Capacity)
                      .IsRequired();

                entity.Property(w => w.CurrentUsage)
                      .IsRequired();

                entity.Property(w => w.IsActive)
                      .HasDefaultValue(true);

                entity.HasOne(w => w.Branch)
                      .WithMany(b => b.Warehouses)
                      .HasForeignKey(w => w.BranchId)
                      .OnDelete(DeleteBehavior.Cascade);

                // NEW (UC-05): Warehouse name must be unique within a branch
                entity.HasIndex(w => new { w.BranchId, w.Name })
                      .IsUnique();
            });

            // ===================== SHOP MANAGER (NEW UC-07) =====================
            modelBuilder.Entity<ShopManager>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(m => m.Email)
                      .HasMaxLength(255);

                entity.Property(m => m.Phone)
                      .HasMaxLength(50);

                entity.Property(m => m.IsActive)
                      .HasDefaultValue(true);

                // (valfritt) om du vill förhindra dubletter:
                // entity.HasIndex(m => m.Email).IsUnique();
                // entity.HasIndex(m => m.Phone).IsUnique();
            });

            // ===================== BRANCH MANAGER (JOIN TABLE) =====================
            modelBuilder.Entity<BranchManager>(entity =>
            {
                // composite key
                entity.HasKey(x => new { x.BranchId, x.ShopManagerId });

                entity.HasOne(x => x.Branch)
                      .WithMany(b => b.BranchManagers) // kräver att du lagt BranchManagers på Branch
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.ShopManager)
                      .WithMany(m => m.BranchManagers) // kräver att du lagt BranchManagers på ShopManager
                      .HasForeignKey(x => x.ShopManagerId)
                      .OnDelete(DeleteBehavior.Cascade);
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

                entity.Property(a => a.Action)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(a => a.EntityType)
                      .IsRequired()
                      .HasMaxLength(200);

                // NEW: om du lagt Timestamp i AuditLog
                // entity.Property(a => a.Timestamp)
                //       .HasDefaultValueSql("SYSUTCDATETIME()");
            });
        }
    }
}

