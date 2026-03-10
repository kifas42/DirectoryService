namespace DirectoryService.Contracts.Departments;

public record TopDepartmentsResponse(IReadOnlyList<TopDepartmentDto> TopDepartments, int Count);

public record TopDepartmentDto
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int PositionsCount { get; init; }
}