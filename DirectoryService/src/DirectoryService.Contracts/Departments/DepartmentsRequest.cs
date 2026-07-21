namespace DirectoryService.Contracts.Departments;

public record RootDepartmentsRequest(int? Page = 1, int? Size = 20, int? Prefetch = 3);

public record ChildDepartmentsRequest(int? Page = 1, int? Size = 20);

public record GetDepartmentsRequest
{
    public string? Search { get; init; } = null;

    public bool? IsActive { get; init; } = null;

    public Guid[]? LocationIds { get; init; } = null;

    public Guid[]? ExcludeIds { get; init; } = null;

    public Guid? ParentId { get; init; } = null;

    public int? Page { get; init; } = 1;

    public int? PageSize { get; init; } = 20;

    public string? SortBy { get; init; } = "name";

    public string? SortOrder { get; init; } = "asc";
}