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
            : base(options) { }

        // ===================== USERS =====================
        public DbSet<User> Users => Set<User>();
        public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();

        // ===================== SHOPS =====================
        public DbSet<ShopCompany> ShopCompanies => Set<ShopCompany>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<BranchManager> BranchManagers => Set<BranchManager>();
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
        public DbSet<LoginAuditLog> LoginAuditLogs => Set<LoginAuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== USER =====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.UserEmail)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.HasIndex(u => u.UserEmail).IsUnique();

                entity.Property(u => u.PasswordHash).IsRequired(false);
                entity.Property(u => u.Phone).HasMaxLength(20).IsRequired(false);

                entity.Property(u => u.Role).IsRequired();
                entity.Property(u => u.OnboardingCompleted).HasDefaultValue(false);
                entity.Property(u => u.IsActive).HasDefaultValue(true);
            });

            // User -> Branch (Employees)
            modelBuilder.Entity<User>()
                .HasOne<Branch>()
                .WithMany(b => b.Employees)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===================== LOGIN AUDIT =====================
            modelBuilder.Entity<LoginAuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Identifier).IsRequired().HasMaxLength(256);
                entity.Property(x => x.Role).IsRequired().HasMaxLength(50);
                entity.Property(x => x.Success).IsRequired();
                entity.Property(x => x.Timestamp).IsRequired();
            });

            // ===================== VERIFICATION CODE =====================
            modelBuilder.Entity<VerificationCode>(entity =>
            {
                entity.HasKey(vc => vc.Id);

                entity.Property(vc => vc.Identifier).IsRequired().HasMaxLength(256);
                entity.Property(vc => vc.Code).IsRequired().HasMaxLength(10);
                entity.Property(vc => vc.CreatedAt).IsRequired();
                entity.Property(vc => vc.ExpiresAt).IsRequired();
                entity.Property(vc => vc.Used).IsRequired();

                entity.HasIndex(vc => new { vc.Identifier, vc.Code });
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
                entity.Property(b => b.Name).IsRequired();
                entity.Property(b => b.City).IsRequired();
                entity.Property(b => b.Address).IsRequired();
            });

            // ===================== BRANCH MANAGER =====================
            modelBuilder.Entity<BranchManager>(entity =>
            {
                entity.HasKey(x => new { x.BranchId, x.ShopManagerId });

                entity.HasOne(x => x.Branch)
                      .WithMany(b => b.BranchManagers)
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ShopManager)
                      .WithMany()
                      .HasForeignKey(x => x.ShopManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== WAREHOUSE =====================
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.Property(w => w.Name).IsRequired();
                entity.Property(w => w.Capacity).IsRequired();

                entity.HasOne(w => w.Branch)
                      .WithMany(b => b.Warehouses)
                      .HasForeignKey(w => w.BranchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== VEHICLE =====================
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.Property(v => v.PlateNumber)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(v => new { v.OwnerId, v.PlateNumber }).IsUnique();

                entity.Property(v => v.Make).HasMaxLength(50);
                entity.Property(v => v.Model).HasMaxLength(50);

                entity.Property(v => v.IsActive).HasDefaultValue(true);
                entity.Property(v => v.HasCompletedService).HasDefaultValue(false);
                entity.Property(v => v.DearchivedAt).IsRequired(false);

                entity.HasMany(v => v.TireSets)
                      .WithOne()
                      .HasForeignKey(t => t.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== TIRE SET =====================
            modelBuilder.Entity<TireSet>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Size).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Brand).IsRequired().HasMaxLength(80);
                entity.Property(t => t.Notes).HasMaxLength(500);
                entity.Property(t => t.IsLocked).HasDefaultValue(false);

                entity.HasIndex(t => new { t.VehicleId, t.TireType }).IsUnique();
            });

            // ===================== VEHICLE STORAGE PREF =====================
            modelBuilder.Entity<VehicleStoragePreference>(entity =>
            {
                entity.HasKey(vs => vs.Id);
                entity.HasIndex(vs => new { vs.VehicleId, vs.BranchId }).IsUnique();
            });

            // ===================== BOOKING =====================
     
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(x => x.Id);

                // Om ServiceType/Status/TireType är enums -> lagra som int
                entity.Property(x => x.ServiceType).HasConversion<int>().IsRequired();
                entity.Property(x => x.Status).HasConversion<int>().IsRequired();
                entity.Property(x => x.TireType).HasConversion<int>(); // lägg IsRequired() om den måste finnas

                entity.HasOne<Vehicle>()
                      .WithMany()
                      .HasForeignKey(x => x.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Branch>()
                      .WithMany()
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Restrict); // eller Cascade om ni vill, men Restrict brukar vara säkrare

                // Index för branch + datum (från din BookingConfiguration)
                entity.HasIndex(x => new { x.BranchId, x.AppointmentDate });
            });


            // ===================== INSPECTION =====================
            modelBuilder.Entity<Inspection>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.HasOne<Booking>()
                      .WithOne()
                      .HasForeignKey<Inspection>(i => i.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== INSPECTION REPORT =====================
            modelBuilder.Entity<InspectionReport>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasMany(r => r.Photos)
                      .WithOne()
                      .HasForeignKey(p => p.InspectionReportId)
                      .OnDelete(DeleteBehavior.Cascade);
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
