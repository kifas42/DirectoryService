using System.Data.Common;
using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis").Build();
    private ConnectionMultiplexer? _redisConnection;

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;
    private string _redisConnStr = null!;

    public IDatabase RedisDb => _redisConnection?.GetDatabase()
                                ?? throw new InvalidOperationException("Redis not initialized.");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            var dbConn = _dbContainer.GetConnectionString();

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{ApplicationDbContext.DATABASE}"] = dbConn,
                [$"ConnectionStrings:{CacheConstants.SECTION_NAME}"] = _redisConnStr
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var bgDescriptor = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .SingleOrDefault(d => d.ImplementationType == typeof(CleanupInactiveRecordsBackgroundService));

            if (bgDescriptor != null) services.Remove(bgDescriptor);

            services.RemoveAll<IDbConnectionFactory>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IOptions<CleanupInactiveRecordsOptions>>();
            services.RemoveAll<IConfigureOptions<HybridCacheOptions>>();
            services.RemoveAll<IDistributedCache>();
            services.RemoveAll<IConnectionMultiplexer>();
            services.RemoveAll<HybridCache>();

            services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            services.AddOptions<CleanupInactiveRecordsOptions>()
                .Configure(opts => opts.RetentionDays = 0);

            services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

            services.AddStackExchangeRedisCache(options => { options.Configuration = _redisConnStr; });

            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(1),
                    Flags = HybridCacheEntryFlags.DisableLocalCache
                };
            });
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        await InitializeRespawner();

        _redisConnStr = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";
        _redisConnection = await ConnectionMultiplexer.ConnectAsync(_redisConnStr);
    }


    public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(_dbConnection);

    public async Task FlushRedisAsync() => await RedisDb.ExecuteAsync("FLUSHDB");

    private async Task InitializeRespawner() =>
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_redisConnection != null)
            await _redisConnection.DisposeAsync();

        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();

        await _dbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();

        await base.DisposeAsync();
    }
}