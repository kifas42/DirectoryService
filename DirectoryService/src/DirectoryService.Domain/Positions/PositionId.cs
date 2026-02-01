namespace DirectoryService.Domain.Positions;

public sealed record PositionId(Guid Value)
{
    public static PositionId New() => new (Guid.NewGuid());
}