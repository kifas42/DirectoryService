using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest LocationRequest) : ICommand;

public sealed class CreateLocationHandler : ICommandHandler<LocationId, CreateLocationCommand>
{
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly ILocationRepository _locationRepository;
    private readonly IValidator<CreateLocationRequest> _validator;

    public CreateLocationHandler(
        ILogger<CreateLocationHandler> logger,
        ILocationRepository locationRepository,
        IValidator<CreateLocationRequest> validator)
    {
        _logger = logger;
        _locationRepository = locationRepository;
        _validator = validator;
    }

    public async Task<Result<LocationId, Errors>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var locationRequest = command.LocationRequest;

        var validationResult = await _validator.ValidateAsync(locationRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var addressResult = Address.Create(
            locationRequest.Address.OfficeNumber,
            locationRequest.Address.BuildingNumber,
            locationRequest.Address.Street,
            locationRequest.Address.City,
            locationRequest.Address.StateOrProvince,
            locationRequest.Address.Country,
            locationRequest.Address.PostalCode);
        var tzResult = Timezone.Create(locationRequest.Timezone);

        var locationResult = Location.Create(
            locationRequest.Name,
            addressResult.Value,
            tzResult.Value);

        if (locationResult.IsFailure)
        {
            _logger.LogError("Failed to create location: {ErrorMessage}", locationResult.Error);
            return new Errors([locationResult.Error]);
        }

        var createLocationResult = await _locationRepository.AddAsync(locationResult.Value, cancellationToken);

        if (createLocationResult.IsFailure)
        {
            _logger.LogError("Failed to add location: {ErrorMessage}", createLocationResult.Error);
            return new Errors([createLocationResult.Error]);
        }

        _logger.LogInformation("Added location: {LocationId}", createLocationResult.Value);

        return createLocationResult.Value;
    }
}