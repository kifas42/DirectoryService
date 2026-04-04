using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ILogger<PositionRepository> _logger;
    private readonly ApplicationDbContext _dbContext;

    public PositionRepository(ApplicationDbContext dbContext, ILogger<PositionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<PositionId, Error>> AddAsync(Position position, CancellationToken cancellationToken)
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
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } when
                    pgEx.ConstraintName.Contains(
                        IndexConstants.POSITION_ACTIVE_NAME,
                        StringComparison.CurrentCultureIgnoreCase) =>
                    Error.Conflict("unique.conflict", "Name conflict"),
                _ => Error.Failure(null, "database error. check logs")
            };
        }
        catch (OperationCanceledException ex)
        {
            return Error.Failure(null, "OperationCanceled");
        }
        catch (Exception ex)
        {
            _logger.LogError("AddAsync Error: {Message}", ex.Message);
            return Error.Failure(null, "database error. check logs");
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
            var dbConn = _dbContext.Database.GetDbConnection();

            int updated = await dbConn.ExecuteAsync(
                sql,
                new { departmentId = departmentId.Value });

            _logger.LogInformation("Deleted(soft) {Count} positions", updated);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to soft delete orphans positions: {Message}", e.Message);
            return Error.Failure("delete.positions", "Failed to soft delete orphans positions");
        }
    }
}