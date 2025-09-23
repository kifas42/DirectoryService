using CSharpFunctionalExtensions;
using DirectoryService.Domain;

namespace DirectoryService.Application;

public interface ILocationRepository
{
    public Task<Result<Guid, string>> AddAsync(Location location, CancellationToken cancellationToken);
}