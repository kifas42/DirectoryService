namespace DirectoryService.Contracts.Positions;

public class GetPositionRequest
{
    public Guid[]? DepartmentIds { get; init; } = null;

    public string? Search { get; init; } = null;

    public bool? IsActive { get; init; } = null;

    public string? Cursor { get; init; } = null;

    public int Limit { get; init; } = 10;

    public string SortBy { get; init; } = "name";

    public string SortOrder { get; init; } = "asc";
}