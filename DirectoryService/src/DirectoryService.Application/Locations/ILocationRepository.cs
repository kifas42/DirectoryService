using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository
{
    public Task<Result<LocationId, Error>> AddAsync(Location location, CancellationToken cancellationToken);
}