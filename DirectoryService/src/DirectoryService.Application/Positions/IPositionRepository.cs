using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionRepository
{
    Task<Result<PositionId, Error>> AddAsync(Position position, CancellationToken cancellationToken);

    Task<UnitResult<Error>> SoftDeleteOrphans(DepartmentId departmentId, CancellationToken cancellationToken);
}