using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department;

public class DepartmentLocation
{
    public DepartmentLocation(Guid departmentId, Guid locationId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public Guid Id { get; }

    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }

    public Result ChangeLocationId(Guid newLocationId)
    {
        LocationId = newLocationId;
        return Result.Success();

        // TBD: валидация и возврат ошибок
    }

    public Result ChangeDepartmentId(Guid newDepartmentId)
    {
        DepartmentId = newDepartmentId;
        return Result.Success();

        // TBD: валидация и возврат ошибок
    }
}