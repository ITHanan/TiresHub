using ApplicationLayer.Features.Bookings.Commands.CreateBooking;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Availability;
using ApplicationLayer.Interfaces.Bookings;
using ApplicationLayer.Interfaces.Identity;
using InfrastructureLayer.Helpers;
using InfrastructureLayer.Identity;
using InfrastructureLayer.Persistence;
using InfrastructureLayer.Repositories;
using InfrastructureLayer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
            services.AddScoped<IJwtGenerator, JWTGenerator>();
            services.AddScoped<ILoginAuditRepository, LoginAuditRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<ITireSetRepository, TireSetRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            // Register MediatR handlers from ApplicationLayer assembly using configuration lambda
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly));

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBranchAvailabilityRepository, BranchAvailabilityRepository>();

            // plus dina:
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IBranchRepository, BranchRepository>();

            // Notification service
            services.AddScoped<ApplicationLayer.Interfaces.INotificationService, NotificationService>();

            return services;
        }
    }
}
