using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.Departments;

public sealed class DepartmentLocation
{
    public DepartmentLocation(Guid id, DepartmentId departmentId, LocationId locationId)
    {
        Id = id;
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    private DepartmentLocation() { }

    public Guid Id { get; }

    public DepartmentId DepartmentId { get; private set; } = null!;

    public LocationId LocationId { get; private set; } = null!;

    public Result ChangeLocationId(LocationId newLocationId)
    {
        LocationId = newLocationId;
        return Result.Success();

        // TBD: валидация и возврат ошибок
    }

    public Result ChangeDepartmentId(DepartmentId newDepartmentId)
    {
        DepartmentId = newDepartmentId;
        return Result.Success();

        // TBD: валидация и возврат ошибок
    }
}