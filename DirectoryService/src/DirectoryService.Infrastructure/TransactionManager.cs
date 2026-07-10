using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure;

public class TransactionManager : ITransactionManager
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public TransactionManager(ApplicationDbContext context, ILogger<TransactionManager> logger,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            ILogger<TransactionScope> logger = _loggerFactory.CreateLogger<TransactionScope>();
            TransactionScope transactionScope = new(transaction.GetDbTransaction(), logger);
            return transactionScope;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to begin transaction");
            return Error.Failure(SharedErrorCodes.System.Database.TransactionFailed, "Failed to begin transaction");
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(
                ex,
                "Concurrency conflict detected. Entities: {Entities}",
                string.Join(", ", ex.Entries.Select(e => e.Entity.GetType().Name)));

            return Error.Conflict(
                SharedErrorCodes.System.Database.ConcurrencyConflict,
                "Запись была изменена другим пользователем. Обновите данные и попробуйте снова.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error");
            return Error.Failure(
                SharedErrorCodes.System.Database.OperationFailed,
                "Ошибка при сохранении в базу данных.");
        }
        catch (Exception ex)
        {
            // Неожиданные ошибки
            _logger.LogError(ex, "Failed to save changes");
            return Error.Failure(
                SharedErrorCodes.System.Database.SaveChangesFailed,
                "Не удалось сохранить изменения.");
        }
    }
}