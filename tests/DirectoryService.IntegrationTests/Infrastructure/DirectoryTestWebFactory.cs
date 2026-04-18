using System.Data.Common;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            var testConnectionString = _dbContainer.GetConnectionString();

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{ApplicationDbContext.DATABASE}"] = testConnectionString
            });
        });


        builder.ConfigureTestServices(services =>
        {
            var descriptor = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .SingleOrDefault(d => d.ImplementationType == typeof(CleanupInactiveRecordsBackgroundService));

            if (descriptor != null)
                services.Remove(descriptor);

            services.RemoveAll<IDbConnectionFactory>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IOptions<CleanupInactiveRecordsOptions>>();
            services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
            {
                var connectionString = _dbContainer.GetConnectionString();

                options.UseNpgsql(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
            services.AddOptions<CleanupInactiveRecordsOptions>()
                .Configure(opts => { opts.RetentionDays = 0; });
            services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        await InitializeRespawner();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions()
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }
}