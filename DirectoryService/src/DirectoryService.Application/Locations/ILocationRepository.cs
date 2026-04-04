using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository
{
    Task<Result<LocationId, Error>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<bool> IsAllExistAndActive(IEnumerable<LocationId> departmentIds);

    Task<UnitResult<Error>> SoftDeleteOrphans(DepartmentId departmentId, CancellationToken cancellationToken);
}