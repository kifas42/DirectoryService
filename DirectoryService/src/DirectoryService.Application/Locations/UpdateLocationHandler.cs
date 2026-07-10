using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations;

public record UpdateLocationCommand(Guid Id, CreateLocationRequest LocationRequest) : ICommand;

public class UpdateLocationHandler : ICommandHandler<UpdateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<UpdateLocationHandler> _logger;
    private readonly IValidator<CreateLocationRequest> _validator;
    private readonly ITransactionManager _transactionManager;

    public UpdateLocationHandler(
        ILocationRepository locationRepository,
        ILogger<UpdateLocationHandler> logger,
        IValidator<CreateLocationRequest> validator,
        ITransactionManager transactionManager)
    {
        _locationRepository = locationRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> Handle(
        UpdateLocationCommand command,
        CancellationToken cancellationToken)
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

        if (addressResult.IsFailure)
        {
            _logger.LogError("Failed to update location: {ErrorMessage}", addressResult.Error);
            return addressResult.Error;
        }


        Result<Timezone, Error> tzResult = Timezone.Create(command.LocationRequest.Timezone);

        if (tzResult.IsFailure)
        {
            _logger.LogError("Failed to update location: {ErrorMessage}", tzResult.Error);
            return tzResult.Error;
        }

        var locationId = new LocationId(command.Id);

        var locationResult = await _locationRepository.GetAsync(locationId, cancellationToken);


        if (locationResult.IsFailure)
        {
            // ex NotFound
            _logger.LogError("Failed to get location: {ErrorMessage}", locationResult.Error);
            return locationResult.Error;
        }

        locationResult.Value.SetAddress(addressResult.Value);
        var setNameResult = locationResult.Value.SetName(command.LocationRequest.Name);

        if (setNameResult.IsFailure)
        {
            _logger.LogError("Failed to update location: {ErrorMessage}", setNameResult.Error);
            return setNameResult.Error;
        }

        locationResult.Value.SetTimeZone(tzResult.Value);

        UnitResult<Error> saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (!saveResult.IsFailure)
        {
            return UnitResult.Success<Error>();
        }

        _logger.LogError("Failed to save changed location: {ErrorMessage}", saveResult.Error);
        return saveResult.Error;
    }
}