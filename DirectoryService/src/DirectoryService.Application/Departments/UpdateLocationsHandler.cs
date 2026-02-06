using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record UpdateLocationCommand(Guid DepartmentId, UpdateLocationsRequest Request) : ICommand;

public class UpdateLocationsHandler : ICommandHandler<int, UpdateLocationCommand>
{
    private readonly ILogger<UpdateLocationsHandler> _logger;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IValidator<UpdateLocationsRequest> _validator;
    private readonly ITransactionManager _transactionManager;

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

    public async Task<Result<int, Errors>> Handle(
        UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            errors.AddRange(validationResult.ToErrors());
        }


        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var departmentResult = _departmentRepository.GetById(new DepartmentId(command.DepartmentId));
        if (departmentResult.IsFailure)
        {
            errors.Add(departmentResult.Error);
        }

        if (!departmentResult.Value.IsActive)
            errors.Add(Error.Failure("is-active", $"department {command.DepartmentId} is not active"));

        var locationIds = command.Request.LocationIds
            .Select(g => new LocationId(g)).ToList();

        if (!_locationRepository.IsAllExistAndActive(locationIds))
        {
            errors.Add(Error.NotFound("find.active.locations", "Locations not found", null));
        }


        if (errors.Count != 0)
        {
            transactionScope.Rollback();
            return new Errors(errors);
        }

        var departmentLocations = command.Request.LocationIds
            .Select(g => new DepartmentLocation(Guid.NewGuid(), departmentResult.Value.Id, new LocationId(g))).ToList();
        var result = departmentResult.Value.SetLocations(departmentLocations);

        if (result.IsFailure)
        {
            transactionScope.Rollback();
            return result.Error.ToErrors();
        }

        var deleteLocationsResult =
            await _departmentRepository.DeleteLocationsAsync(departmentResult.Value.Id, cancellationToken);
        if (deleteLocationsResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteLocationsResult.Error.ToErrors();
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error.ToErrors();
        }

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            return commitedResult.Error.ToErrors();
        }

        return departmentLocations.Count;
    }
}