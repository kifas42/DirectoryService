using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PositionRepository> _logger;

    public PositionRepository(ApplicationDbContext dbContext, ILogger<PositionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<PositionId, Error>> AddAsync(
        Position position,
        CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Positions.Add(position);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return position.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            return pgEx switch
            {
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                    when pgEx.ConstraintName.Contains(
                        IndexConstants.POSITION_ACTIVE_NAME,
                        StringComparison.OrdinalIgnoreCase) =>
                    Error.Conflict(
                        DomainErrorCodes.Position.NameConflict,
                        "Позиция с таким названием уже существует в этом департаменте",
                        "name"),

                _ => HandleUnexpectedDbError(ex)
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation was canceled while adding position {PositionName}", position.Name);
            return Error.Failure(
                SharedErrorCodes.System.OperationCanceled,
                "Операция была прервана");
        }
        catch (Exception ex)
        {
            return HandleUnexpectedDbError(ex);
        }
    }

    public async Task<UnitResult<Error>> SoftDeleteOrphans(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        string sql = """
                     UPDATE positions
                     SET is_active = FALSE,
                         updated_at = NOW(),
                         deleted_at = NOW()
                     WHERE id IN (
                         SELECT position_id FROM department_position WHERE department_id = @departmentId
                     )
                       AND NOT EXISTS (
                         SELECT 1
                         FROM department_position dl2
                                  JOIN departments d2 ON d2.id = dl2.department_id
                         WHERE dl2.position_id = positions.id
                           AND d2.is_active = TRUE
                     );
                     """;

        try
        {
            DbConnection dbConn = _dbContext.Database.GetDbConnection();

            int updated = await dbConn.ExecuteAsync(
                sql,
                new { departmentId = departmentId.Value },
                commandTimeout: 30);

            _logger.LogInformation(
                "Successfully soft-deleted {Count} orphan positions for DepartmentId {DepartmentId}",
                updated,
                departmentId.Value);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to soft-delete orphan positions for DepartmentId {DepartmentId}",
                departmentId.Value);

            return Error.Failure(
                DomainErrorCodes.Position.OrphanDeleteFailed,
                "Не удалось удалить связанные позиции. Пожалуйста, попробуйте позже.");
        }
    }

    private Error HandleUnexpectedDbError(Exception ex)
    {
        _logger.LogError(ex, "Unexpected database error while adding position");
        return Error.Failure(
            SharedErrorCodes.System.Database.OperationFailed,
            "Произошла внутренняя ошибка базы данных. Пожалуйста, попробуйте позже.");
    }
}