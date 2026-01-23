namespace DirectoryService.Contracts.Locations;

public class CreateLocationRequest
{
    public string Name { get; init; } = string.Empty;

    public AddressDto Address { get; init; } = null!;

    public string Timezone { get; init; } = string.Empty;
}