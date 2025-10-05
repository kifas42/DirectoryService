using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Domain;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ILogger<LocationRepository> _logger;
    private readonly ApplicationDbContext _dbContext;

    public LocationRepository(ILogger<LocationRepository> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("AddAsync Error: {Message}", ex.Message);
            return Error.Failure(null, "backend error. check logs");
        }

        return location.Id;
    }
}