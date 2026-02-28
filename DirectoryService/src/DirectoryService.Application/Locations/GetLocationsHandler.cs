using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations;

public class GetLocationsHandler
{
    private readonly IReadDbContext _readDbContext;

    public GetLocationsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetLocationsDto?> Handle(
        GetLocationQuery query,
        CancellationToken cancellationToken)
    {
        var location = await _readDbContext.LocationsRead
            .FirstOrDefaultAsync(l => l.Name == query.Search, cancellationToken: cancellationToken);

        if (location is null)
        {
            return null;
        }

        return new GetLocationsDto()
        {
            Name = location.Name,
            OfficeNumber = location.Address.OfficeNumber,
            BuildingNumber = location.Address.BuildingNumber,
            Street = location.Address.Street,
            City = location.Address.City,
            StateOrProvince = location.Address.StateOrProvince,
            Country = location.Address.Country,
            PostalCode = location.Address.PostalCode,
            Timezone = location.Timezone.Value,
        };
    }
}