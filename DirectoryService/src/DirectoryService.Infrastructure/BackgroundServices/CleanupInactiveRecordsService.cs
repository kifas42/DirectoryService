using System.Data;
using Dapper;
using DirectoryService.Application.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupInactiveRecordsService
{
    private const string DELETE_SIMPLE_SQL = """
                                           DELETE FROM {Table}
                                           WHERE id IN (
                                               SELECT id FROM {Table}
                                               WHERE is_active = false AND deleted_at < @CutoffDate
                                               ORDER BY id
                                               LIMIT @BatchSize
                                           );
                                           """;

    private const string SELECT_BATCH_IDS_SQL = """
                                             SELECT id FROM departments
                                             WHERE is_active = false AND deleted_at < @CutoffDate
                                             ORDER BY id
                                             LIMIT @BatchSize;
                                             """;

    private const string REPARENT_SQL = """
                                       UPDATE departments AS d
                                       SET path = subpath(d.path, nlevel(dep.path)),
                                           parent_id = CASE WHEN d.parent_id = ANY(@IdsToDelete) THEN NULL ELSE d.parent_id END,
                                           updated_at = NOW()
                                       FROM departments AS dep
                                       WHERE dep.id = ANY(@IdsToDelete)
                                         AND d.path <@ dep.path 
                                         AND d.id <> ALL(@IdsToDelete);
                                       """;

    private const string DELETE_DEPARTMENTS_SQL = "DELETE FROM departments WHERE id = ANY(@IdsToDelete);";
    private static readonly string[] _tables = ["locations", "positions", "departments"];

    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<CleanupInactiveRecordsService> _logger;
    private readonly CleanupInactiveRecordsOptions _options;

    public CleanupInactiveRecordsService(
        ILogger<CleanupInactiveRecordsService> logger,
        IDbConnectionFactory dbConnectionFactory,
        IOptions<CleanupInactiveRecordsOptions> options)
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
        _options = options.Value;
    }

    public async Task RunCleanupAsync(CancellationToken stoppingToken)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(stoppingToken);

        DateTime cutoff = DateTime.UtcNow.AddDays(-_options.RetentionDays);
        _logger.LogInformation("Начало очистки. Cutoff: {Cutoff}", cutoff);

        foreach (string table in _tables)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

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

        string sql = DELETE_SIMPLE_SQL.Replace("{Table}", table);
        int total = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                int deleted = await connection.ExecuteAsync(
                    sql,
                    new { CutoffDate = cutoff, _options.BatchSize },
                    transaction,
                    30);

                transaction.Commit();
                total += deleted;

                if (deleted < _options.BatchSize)
                {
                    break;
                }

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
        int total = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                Guid[] ids = (await connection.QueryAsync<Guid>(
                    SELECT_BATCH_IDS_SQL,
                    new { CutoffDate = cutoff, _options.BatchSize },
                    transaction,
                    10)).ToArray();

                if (ids.Length == 0)
                {
                    break;
                }

                await connection.ExecuteAsync(
                    REPARENT_SQL,
                    new { IdsToDelete = ids },
                    transaction,
                    30);

                await connection.ExecuteAsync(
                    DELETE_DEPARTMENTS_SQL,
                    new { IdsToDelete = ids },
                    transaction,
                    30);

                transaction.Commit();
                total += ids.Length;

                if (ids.Length < _options.BatchSize)
                {
                    break;
                }

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

        _logger.LogInformation("departments: удалено {Total} записей", total);
    }
}