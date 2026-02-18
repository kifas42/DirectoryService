using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<DepartmentId, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdIsActive(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<bool> IsAllExistAndActive(IEnumerable<DepartmentId> departmentIds);

    Task<UnitResult<Error>> DeleteLocationsAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateDepartmentDescendants(
        Department root,
        Department? newParent,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> LockDepartmentsById(
        DepartmentId departmentId,
        CancellationToken cancellationToken);
}