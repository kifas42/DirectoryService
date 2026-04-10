using System.Data;
using Dapper;
using DirectoryService.Application.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupInactiveRecordsService : BackgroundService
{
    private static readonly string[] _tables = { "locations", "positions", "departments" };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CleanupInactiveRecordsOptions _options;
    private readonly ILogger<CleanupInactiveRecordsService> _logger;

    // Простой DELETE для locations/positions
    private const string DeleteSimpleSql = """
                                           DELETE FROM {Table}
                                           WHERE id IN (
                                               SELECT id FROM {Table}
                                               WHERE is_active = false AND deleted_at < @CutoffDate
                                               ORDER BY id
                                               LIMIT @BatchSize
                                           );
                                           """;

    // Departments: 1) выбираем ID батча
    private const string SelectBatchIdsSql = """
                                             SELECT id FROM departments
                                             WHERE is_active = false AND deleted_at < @CutoffDate
                                             ORDER BY id
                                             LIMIT @BatchSize;
                                             """;

    // Departments: 2) перепривязка потомков
    private const string ReparentSql = """
                                       UPDATE departments AS d
                                       SET path = subpath(d.path, nlevel(dep.path)),
                                           parent_id = CASE WHEN d.parent_id = ANY(@IdsToDelete) THEN NULL ELSE d.parent_id END,
                                           updated_at = NOW()
                                       FROM departments AS dep
                                       WHERE dep.id = ANY(@IdsToDelete)
                                         AND d.path <@ dep.path 
                                         AND d.id <> ALL(@IdsToDelete);
                                       """;

    // Departments: 3) удаление по списку ID
    private const string DeleteDepartmentsSql = "DELETE FROM departments WHERE id = ANY(@IdsToDelete);";

    public CleanupInactiveRecordsService(
        IServiceScopeFactory scopeFactory,
        IOptions<CleanupInactiveRecordsOptions> options,
        ILogger<CleanupInactiveRecordsService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var time = CalculateNextRun();
        _logger.LogInformation("Hello. Next run is {Time}", time);

        await Task.Delay(time, stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        var period = TimeSpan.FromDays(_options.FrequencyDays);
        if (period <= TimeSpan.Zero)
        {
            period = TimeSpan.FromSeconds(1);
            _logger.LogWarning("FrequencyDays <= 0. Установлен тестовый интервал 1 сек.");
        }

        using var timer = new PeriodicTimer(period);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCleanupAsync(stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using var connection = await factory.CreateConnectionAsync(stoppingToken);

        var cutoff = DateTime.UtcNow.AddDays(-_options.RetentionDays);
        _logger.LogInformation("Начало очистки. Cutoff: {Cutoff}", cutoff);

        foreach (string table in _tables)
        {
            if (stoppingToken.IsCancellationRequested) break;
            await CleanupTableAsync(connection, table, cutoff, stoppingToken);
        }
    }

    private async Task CleanupTableAsync(
        IDbConnection connection,
        string table,
        DateTime cutoff,
        CancellationToken cancellationToken)
    {
        if (table == "departments")
        {
            await CleanupDepartmentsWithTreeAsync(connection, cutoff, cancellationToken);
            return;
        }

        var sql = DeleteSimpleSql.Replace("{Table}", table);
        int total = 0, batch = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            batch++;
            using var transaction = connection.BeginTransaction();
            try
            {
                int deleted = await connection.ExecuteAsync(
                    sql,
                    new { CutoffDate = cutoff, BatchSize = _options.BatchSize },
                    transaction,
                    commandTimeout: 30);

                transaction.Commit();
                total += deleted;

                if (deleted < _options.BatchSize) break;
                await Task.Delay(100, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Очистка {Table} прервана. Повтор через {Days} дн.",
                    table, _options.FrequencyDays);
                break;
            }
        }

        _logger.LogInformation("{Table}: удалено {Total} записей", table, total);
    }

    private async Task CleanupDepartmentsWithTreeAsync(
        IDbConnection connection,
        DateTime cutoff,
        CancellationToken cancellationToken)
    {
        int total = 0, batch = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            batch++;
            using var transaction = connection.BeginTransaction();
            try
            {
                var ids = (await connection.QueryAsync<Guid>(
                    SelectBatchIdsSql,
                    new { CutoffDate = cutoff, BatchSize = _options.BatchSize },
                    transaction,
                    commandTimeout: 10)).ToArray();

                if (ids.Length == 0) break;
                
                await connection.ExecuteAsync(
                    ReparentSql,
                    new { IdsToDelete = ids },
                    transaction,
                    commandTimeout: 30);
                
                await connection.ExecuteAsync(
                    DeleteDepartmentsSql,
                    new { IdsToDelete = ids },
                    transaction,
                    commandTimeout: 30);

                transaction.Commit();
                total += ids.Length;

                if (ids.Length < _options.BatchSize) break;
                await Task.Delay(100, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Очистка departments прервана. Повтор через {Days} дн.",
                    _options.FrequencyDays);
                break;
            }
        }

        _logger.LogInformation("✅ departments: удалено {Total} записей", total);
    }

    private TimeSpan CalculateNextRun()
    {
        var now = DateTime.UtcNow;
        var next = now.Date.Add(_options.TriggerTime.ToTimeSpan());
        if (next <= now) next = next.AddDays(1);
        return next - now;
    }
}