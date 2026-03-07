namespace DirectoryService.Contracts.Locations;

public record GetLocationDto()
{
    public string Name { get; init; } = string.Empty;

    public string OfficeNumber { get; init; } = string.Empty;

    public string BuildingNumber { get; init; } = string.Empty;

    public string Street { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string? StateOrProvince { get; init; } = null;

    public string Country { get; init; } = string.Empty;

    public string? PostalCode { get; init; } = null;

    public string Timezone { get; init; } = null!;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}

public record PaginationLocationResponse(IReadOnlyList<GetLocationDto> Locations, int TotalCount);