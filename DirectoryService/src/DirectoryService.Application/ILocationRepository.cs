using CSharpFunctionalExtensions;
using DirectoryService.Domain;
using Shared;

namespace DirectoryService.Application;

public interface ILocationRepository
{
    public Task<Result<LocationId, Error>> AddAsync(Location location, CancellationToken cancellationToken);
}