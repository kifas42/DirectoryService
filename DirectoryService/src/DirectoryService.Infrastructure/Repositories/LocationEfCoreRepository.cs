using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Domain;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationEfCoreRepository : ILocationRepository
{
    private readonly ILogger<LocationEfCoreRepository> _logger;
    private readonly ILocationDbContext _dbContext;

    public LocationEfCoreRepository(ILogger<LocationEfCoreRepository> logger, ILocationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, string>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id;
    }
}