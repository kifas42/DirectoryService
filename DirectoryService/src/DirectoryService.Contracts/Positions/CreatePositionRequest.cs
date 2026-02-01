namespace DirectoryService.Contracts.Positions;

public class CreatePositionRequest
{
    public string Name { get; init; }

    public string? Description { get; init; }

    public Guid[] DepartmentIds { get; init; }
}