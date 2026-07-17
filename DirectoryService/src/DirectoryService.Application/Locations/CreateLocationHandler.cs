using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations;

public record CreateLocationCommand(CreateLocationRequest LocationRequest) : ICommand;

public sealed class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<CreateLocationHandler> _logger;
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

    public async Task<Result<Guid, Error>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command.LocationRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        Result<Address, Error> addressResult = Address.Create(
            command.LocationRequest.Address.OfficeNumber,
            command.LocationRequest.Address.BuildingNumber,
            command.LocationRequest.Address.Street,
            command.LocationRequest.Address.City,
            command.LocationRequest.Address.StateOrProvince,
            command.LocationRequest.Address.Country,
            command.LocationRequest.Address.PostalCode);
        Result<Timezone, Error> tzResult = Timezone.Create(command.LocationRequest.Timezone);

        Result<Location, Error> locationResult = Location.Create(
            LocationId.New(),
            command.LocationRequest.Name,
            addressResult.Value,
            tzResult.Value);

        if (locationResult.IsFailure)
        {
            _logger.LogError("Failed to create location: {ErrorMessage}", locationResult.Error);
            return locationResult.Error;
        }

        Result<LocationId, Error> createLocationResult =
            await _locationRepository.AddAsync(locationResult.Value, cancellationToken);

        if (createLocationResult.IsFailure)
        {
            _logger.LogError("Failed to add location: {ErrorMessage}", createLocationResult.Error);
            return createLocationResult.Error;
        }

        _logger.LogInformation("Added location: {LocationId}", createLocationResult.Value);

        return createLocationResult.Value.Value;
    }
}