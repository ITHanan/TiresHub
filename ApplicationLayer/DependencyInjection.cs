using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ApplicationLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
            services.AddAutoMapper(assembly);

            services.AddValidatorsFromAssembly(assembly);
            // existing generic validation behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ApplicationLayer.Behaviors.ValidationBehavior<,>));
            // register branch ownership validator behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ApplicationLayer.Behaviors.ValidateBranchOwnershipBehavior<,>));

            return services;
        }
    }
}
