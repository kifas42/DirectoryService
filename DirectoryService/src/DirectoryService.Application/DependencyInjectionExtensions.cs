using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjectionExtensions).Assembly;

        string redisConnectionString = configuration.GetConnectionString("Redis")
                                       ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
        services.AddOptions<HybridCacheOptions>()
            .Bind(configuration.GetSection("HybridCache"));

        services.AddValidatorsFromAssembly(assembly);
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes
                .AssignableToAny(typeof(IQueryHandler<,>), typeof(IQueryHandler<>)))
            .AsSelfWithInterfaces().WithScopedLifetime());
        services.AddScoped<GetLocationsHandler>();

        var hybridCacheOptions = configuration
            .GetSection("HybridCache")
            .Get<HybridCacheOptions>() ?? new HybridCacheOptions();

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions()
            {
                LocalCacheExpiration = hybridCacheOptions.LocalCacheExpiration,
                Expiration = hybridCacheOptions.Expiration,
            };
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });
        return services;
    }
}