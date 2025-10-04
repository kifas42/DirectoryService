using CSharpFunctionalExtensions;
using DirectoryService.Domain;
using Shared;

namespace DirectoryService.Application;

public interface ILocationRepository
{
    public Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken);
}