using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository
{
    public Task<Result<DepartmentId, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    public Result<Department, Error> GetById(DepartmentId departmentId);

    public bool IsAllExistAndActive(IEnumerable<DepartmentId> departmentIds);

    Task<UnitResult<Error>> DeleteLocationsAsync(DepartmentId departmentId, CancellationToken cancellationToken);
}