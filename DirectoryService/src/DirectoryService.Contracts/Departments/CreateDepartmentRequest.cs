namespace DirectoryService.Contracts.Departments;

public class CreateDepartmentRequest
{
    public string Name { get; init; }

    public string Identifier { get; init; }

    public Guid? ParentId { get; init; }

    public Guid[] LocationIds { get; init; }
}