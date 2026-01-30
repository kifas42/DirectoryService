using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;
using Shared;

namespace DirectoryService.Application;

public interface IDepartmentRepository
{
    public Task<Result<DepartmentId, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    public Result<Department, Error> GetById(DepartmentId departmentId);

    //public Result<Department, Error> GetByIdWithParents(DepartmentId departmentId);
}