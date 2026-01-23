using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Domain;
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
}