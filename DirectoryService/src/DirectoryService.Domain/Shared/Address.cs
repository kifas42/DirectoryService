namespace DirectoryService.Domain.Shared;

public record Address
{
    public string OfficeNumber { get; }
    public string BuildingNumber { get; }
    public string Street { get; }
    public string City { get; }
    public string? StateOrProvince { get; }
    public string Country { get; }
    public string? PostalCode { get; }

    public Address(
        string officeNumber,
        string buildingNumber,
        string street,
        string city,
        string? stateOrProvince,
        string country,
        string? postalCode)
    {
        OfficeNumber = officeNumber;
        BuildingNumber = buildingNumber;
        Street = street;
        City = city;
        StateOrProvince = stateOrProvince;
        PostalCode = postalCode;
        Country = country;
    }
}