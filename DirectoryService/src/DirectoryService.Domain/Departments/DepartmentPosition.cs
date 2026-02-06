using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.Departments;

public sealed class DepartmentPosition
{
    public DepartmentPosition(Guid id, DepartmentId departmentId, PositionId positionId)
    {
        Id = id;
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    private DepartmentPosition() { }

    public Guid Id { get; }

    public DepartmentId DepartmentId { get; private set; } = null!;

    public PositionId PositionId { get; private set; } = null!;

    public void ChangeLocationId(PositionId newPositionId)
    {
        PositionId = newPositionId;
    }

    public void ChangeDepartmentId(DepartmentId newDepartmentId)
    {
        DepartmentId = newDepartmentId;
    }
}