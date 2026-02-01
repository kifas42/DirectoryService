using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.Departments;

public sealed class DepartmentPosition
{
    public DepartmentPosition(DepartmentId departmentId, PositionId positionId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    private DepartmentPosition() { }

    public Guid Id { get; }

    public DepartmentId DepartmentId { get; private set; } = null!;

    public PositionId PositionId { get; private set; } = null!;

    public Result ChangeLocationId(PositionId newPositionId)
    {
        PositionId = newPositionId;
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