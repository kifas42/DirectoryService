using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();

        services.AddDbContextPool<IReadDbContext, ApplicationDbContext>((sp, options) =>
        {
            string? connectionString = configuration.GetConnectionString(ApplicationDbContext.DATABASE);
            var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            options.UseLoggerFactory(loggerFactory);
        });

        return services;
    }
}