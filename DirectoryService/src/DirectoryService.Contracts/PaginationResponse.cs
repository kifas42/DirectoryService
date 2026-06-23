namespace DirectoryService.Contracts;

public record PaginationResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record InfiniteScrollResponse<T>(
    IReadOnlyList<T> Items,
    string? NextCursor);