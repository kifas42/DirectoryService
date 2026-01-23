namespace DirectoryService.Contracts.Locations;

public class CreateLocationRequest
{
    public string Name { get; init; } = string.Empty;

    public AddressRequest Address { get; init; } = null!;

    public string Timezone { get; init; } = string.Empty;
}