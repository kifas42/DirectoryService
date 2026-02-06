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

    public void ChangeLocationId(LocationId newLocationId)
    {
        LocationId = newLocationId;
    }

    public void ChangeDepartmentId(DepartmentId newDepartmentId)
    {
        DepartmentId = newDepartmentId;
    }
}