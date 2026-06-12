using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record UpdateLocationCommand(Guid DepartmentId, UpdateLocationsRequest Request) : ICommand;

public class UpdateLocationsHandler : ICommandHandler<int, UpdateLocationCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<UpdateLocationsHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateLocationsRequest> _validator;

    public UpdateLocationsHandler(
        ILogger<UpdateLocationsHandler> logger,
        IValidator<UpdateLocationsRequest> validator,
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager, ILocationRepository locationRepository)
    {
        _logger = logger;
        _validator = validator;
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _locationRepository = locationRepository;
    }

    public async Task<Result<int, Error>> Handle(
        UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        Result<ITransactionScope, Error> transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope? transactionScope = transactionScopeResult.Value;

        Result<Department, Error> departmentResult = await _departmentRepository.GetByIdIsActive(
            new DepartmentId(command.DepartmentId),
            cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error;
        }

        List<LocationId> locationIds = command.Request.LocationIds
            .Select(g => new LocationId(g)).ToList();

        if (!await _locationRepository.IsAllExistAndActive(locationIds))
        {
            transactionScope.Rollback();
            return Error.NotFound(DomainErrorCodes.Location.NotFound, "Привязанные локации не существуют или не активны", "locationIds");
        }

        List<DepartmentLocation> departmentLocations = command.Request.LocationIds
            .Select(g => new DepartmentLocation(Guid.NewGuid(), departmentResult.Value.Id, new LocationId(g))).ToList();
        Result<int, Error> result = departmentResult.Value.SetLocations(departmentLocations);

        if (result.IsFailure)
        {
            transactionScope.Rollback();
            return result.Error;
        }

        UnitResult<Error> deleteLocationsResult =
            await _departmentRepository.DeleteLocationsAsync(departmentResult.Value.Id, cancellationToken);
        if (deleteLocationsResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteLocationsResult.Error;
        }

        UnitResult<Error> saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }

        UnitResult<Error> commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            return commitedResult.Error;
        }

        return departmentLocations.Count;
    }
}