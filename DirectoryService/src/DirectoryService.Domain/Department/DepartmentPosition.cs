using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department;

public class DepartmentPosition
{
    public DepartmentPosition(Guid departmentId, Guid positionId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public Guid Id { get; }

    public Guid DepartmentId { get; set; }

    public Guid PositionId { get; set; }

    public Result ChangeLocationId(Guid newPositionId)
    {
        PositionId = newPositionId;
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