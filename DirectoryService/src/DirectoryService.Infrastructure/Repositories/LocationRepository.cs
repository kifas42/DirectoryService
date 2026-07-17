using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(ILogger<LocationRepository> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Result<LocationId, Error>> AddAsync(
        Location location,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Locations.Add(location);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return location.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            return pgEx switch
            {
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                    when pgEx.ConstraintName.Contains(IndexConstants.NAME, StringComparison.OrdinalIgnoreCase) =>
                    Error.Conflict(
                        DomainErrorCodes.Location.NameConflict,
                        "Локация с таким названием уже существует",
                        "name"),
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                    when pgEx.ConstraintName.Contains(IndexConstants.ADDRESS, StringComparison.OrdinalIgnoreCase) =>
                    Error.Conflict(
                        DomainErrorCodes.Location.AddressConflict,
                        "Локация с таким адресом уже существует",
                        null),

                _ => HandleUnexpectedDbError(ex)
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation was canceled while adding location");
            return Error.Failure(
                SharedErrorCodes.System.OperationCanceled,
                "Операция была отменена");
        }
        catch (Exception ex)
        {
            return HandleUnexpectedDbError(ex);
        }
    }

    public async Task<bool> IsAllExistAndActive(IEnumerable<LocationId> departmentIds) =>
        await _dbContext.Locations
            .CountAsync(d => departmentIds.Contains(d.Id) && d.IsActive) == departmentIds.Count();

    public async Task<UnitResult<Error>> SoftDeleteOrphans(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        string sql = """
                     UPDATE locations
                     SET is_active = FALSE,
                         updated_at = NOW(),
                         deleted_at = NOW()
                     WHERE id IN (
                         SELECT location_id FROM department_location WHERE department_id = @departmentId
                     )
                       AND NOT EXISTS (
                         SELECT 1
                         FROM department_location dl2
                                  JOIN departments d2 ON d2.id = dl2.department_id
                         WHERE dl2.location_id = locations.id
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
                "Successfully soft-deleted {Count} orphan locations for DepartmentId {DepartmentId}",
                updated,
                departmentId.Value);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to soft-delete orphan locations for DepartmentId {DepartmentId}",
                departmentId.Value);
            return Error.Failure(
                DomainErrorCodes.Location.OrphanDeleteFailed,
                "Не удалось удалить связанные локации. Пожалуйста, попробуйте позже.");
        }
    }

    public async Task<Result<Location, Error>> GetAsync(LocationId locationId, CancellationToken cancellationToken)
    {
        var location = await _dbContext.Locations.FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        if (location == null) return Error.NotFound();

        return location;
    }

    private Error HandleUnexpectedDbError(Exception ex)
    {
        _logger.LogError(ex, "Unexpected database error while adding location");
        return Error.Failure(
            SharedErrorCodes.System.Database.OperationFailed,
            "Произошла внутренняя ошибка базы данных. Пожалуйста, попробуйте позже.");
    }
}