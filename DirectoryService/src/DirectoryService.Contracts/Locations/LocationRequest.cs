namespace DirectoryService.Contracts.Locations;

public record LocationRequest(
    string OfficeNumber,
    string BuildingNumber,
    string Street,
    string City,
    string? StateOrProvince,
    string Country,
    string? PostalCode);