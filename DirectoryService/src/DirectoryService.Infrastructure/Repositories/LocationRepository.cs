using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly ILogger<LocationRepository> _logger;
    private readonly ApplicationDbContext _dbContext;

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
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } when
                    pgEx.ConstraintName.Contains(IndexConstants.NAME, StringComparison.CurrentCultureIgnoreCase) =>
                    Error.Conflict("unique.conflict", "Name conflict"),
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } when
                    pgEx.ConstraintName.Contains(IndexConstants.ADDRESS, StringComparison.CurrentCultureIgnoreCase) =>
                    Error.Conflict("unique.conflict", "Address conflict"),
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

    public async Task<bool> IsAllExistAndActive(IEnumerable<LocationId> departmentIds)
    {
        return await _dbContext.Locations
            .CountAsync(d => departmentIds.Contains(d.Id) && d.IsActive) == departmentIds.Count();
    }

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
            var dbConn = _dbContext.Database.GetDbConnection();

            int updated = await dbConn.ExecuteAsync(
                sql,
                new { departmentId = departmentId.Value });

            _logger.LogInformation("Deleted(soft) {Count} locations", updated);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to soft delete orphans locations: {Message}", e.Message);
            return Error.Failure("delete.locations", "Failed to soft delete orphans locations");
        }
    }
}