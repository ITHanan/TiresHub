
using ApiLayer.Mappings;
using ApplicationLayer;
using ApplicationLayer.Audit;
using ApplicationLayer.Branches;
using ApplicationLayer.Capacity;
using ApplicationLayer.Common.Mappings;
using ApplicationLayer.Companies;
using ApplicationLayer.Managers;
using ApplicationLayer.Warehouses;
using InfrastructureLayer;
using InfrastructureLayer.Audit;
using InfrastructureLayer.Extensions;
using InfrastructureLayer.Persistence;
using InfrastructureLayer.Service.Branches;
using InfrastructureLayer.Service.Companies;
using Microsoft.EntityFrameworkCore;

namespace ApiLayer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<IBranchService, BranchService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<ICapacityService, CapacityService>();
            builder.Services.AddScoped<IShopManagerService, ShopManagerService>();

            builder.Services.AddScoped<IAuditLogger, AuditLogger>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerWithJwt();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication(); // must come before UseAuthorization

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
