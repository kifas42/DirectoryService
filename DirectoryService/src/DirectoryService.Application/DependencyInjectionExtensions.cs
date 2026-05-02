using System.Reflection;
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
        Assembly assembly = typeof(DependencyInjectionExtensions).Assembly;

        string redisConnectionString = configuration.GetConnectionString(CacheConstants.REDIS_SECTION)
                                       ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheConstants.SECTION_NAME));

        services.AddValidatorsFromAssembly(assembly);

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes
                .AssignableToAny(typeof(IQueryHandler<,>), typeof(IQueryHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.AddScoped<GetLocationsHandler>();

        CacheOptions hybridCacheOptions = configuration
            .GetSection(CacheConstants.SECTION_NAME)
            .Get<CacheOptions>() ?? new CacheOptions();

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
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