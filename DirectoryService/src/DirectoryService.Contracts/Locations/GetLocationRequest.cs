namespace DirectoryService.Contracts.Locations;

public record GetLocationRequest
{
    public Guid[]? DepartmentIds { get; init; } = null;

    public string? Search { get; init; } = null;

    public bool? IsActive { get; init; } = null;

    public int? Page { get; init; } = null;

    public int? PageSize { get; init; } = null;

    public string SortBy { get; init; } = "name";

    public string SortOrder { get; init; } = "asc";
}