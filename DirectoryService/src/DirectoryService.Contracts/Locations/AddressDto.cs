namespace DirectoryService.Contracts.Locations;

public record AddressDto(
    string OfficeNumber,
    string BuildingNumber,
    string Street,
    string City,
    string? StateOrProvince,
    string Country,
    string? PostalCode);