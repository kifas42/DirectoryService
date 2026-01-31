namespace DirectoryService.Domain.Department;

public record DepartmentId(Guid Value)
{
    public static DepartmentId New() => new(Guid.NewGuid());
}