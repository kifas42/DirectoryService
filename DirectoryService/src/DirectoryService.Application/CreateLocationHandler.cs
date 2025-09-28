using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application;

public class CreateLocationHandler
{
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly ILocationRepository _locationRepository;

    public CreateLocationHandler(ILogger<CreateLocationHandler> logger, ILocationRepository locationRepository)
    {
        _logger = logger;
        _locationRepository = locationRepository;
    }

    public async Task<Result<Guid, string>> Handle(
        CreateLocationRequest locationRequest,
        CancellationToken cancellationToken = default)
    {
        var addressResult = Address.Create(
            locationRequest.Address.OfficeNumber,
            locationRequest.Address.BuildingNumber,
            locationRequest.Address.Street,
            locationRequest.Address.City,
            locationRequest.Address.StateOrProvince,
            locationRequest.Address.Country,
            locationRequest.Address.PostalCode);

        if (addressResult.IsFailure)
        {
            _logger.LogError("Failed to verify Address: {ErrorMessage}", addressResult.Error);
            return addressResult.Error;
        }

        var tzResult = Timezone.Create(locationRequest.Timezone);

        if (tzResult.IsFailure)
        {
            _logger.LogError("Failed to create TimeZone: {ErrorMessage}", tzResult.Error);
            return tzResult.Error;
        }

        var locationResult = Location.Create(
            locationRequest.Name,
            addressResult.Value,
            tzResult.Value);

        if (locationResult.IsFailure)
        {
            _logger.LogError("Failed to create location: {ErrorMessage}", locationResult.Error);
        }

        var createLocationResult = await _locationRepository.AddAsync(locationResult.Value, cancellationToken);

        if (createLocationResult.IsFailure)
        {
            _logger.LogError("Failed to add location: {ErrorMessage}", createLocationResult.Error);
        }

        _logger.LogInformation("Added location: {LocationId}", createLocationResult.Value);

        return createLocationResult.Value;
    }
}