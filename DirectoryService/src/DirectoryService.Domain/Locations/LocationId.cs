namespace DirectoryService.Domain.Locations;

public sealed record LocationId(Guid Value)
{
    public static LocationId New() => new(Guid.NewGuid());
}