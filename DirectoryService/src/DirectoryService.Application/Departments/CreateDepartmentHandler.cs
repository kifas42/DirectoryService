using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record CreateDepartmentCommand(CreateDepartmentRequest DepartmentRequest) : ICommand;

public sealed class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<CreateDepartmentRequest> _validator;

    public CreateDepartmentHandler(
        ILogger<CreateDepartmentHandler> logger,
        IDepartmentRepository departmentRepository,
        IValidator<CreateDepartmentRequest> validator)
    {
        _logger = logger;
        _departmentRepository = departmentRepository;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.DepartmentRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var identifierResult = Identifier.Create(command.DepartmentRequest.Identifier);
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error.ToErrors();
        }

        short depth = 0;
        Department? parent = null;
        if (command.DepartmentRequest.ParentId != null)
        {
            var parentId = new DepartmentId(command.DepartmentRequest.ParentId.Value);
            var parentResult = await _departmentRepository.GetByIdIsActive(parentId, cancellationToken);

            if (parentResult.IsFailure)
            {
                _logger.LogError("Parent department not found: {ErrorMessage}", parentResult.Error);
            }

            parent = parentResult.Value;

            depth = (short)(parentResult.Value.Depth + 1);
        }

        var departmentId = DepartmentId.New();
        var departmentLocations = command.DepartmentRequest.LocationIds
            .Select(g => new DepartmentLocation(Guid.NewGuid(), departmentId, new LocationId(g)));

        var departmentResult = Department.Create(
            departmentId,
            command.DepartmentRequest.Name,
            identifierResult.Value,
            parent,
            depth,
            [],
            departmentLocations);

        if (departmentResult.IsFailure)
        {
            _logger.LogError("Failed to create department: {ErrorMessage}", departmentResult.Error);
            return departmentResult.Error.ToErrors();
        }

        var createDepartmentResult = await _departmentRepository.AddAsync(departmentResult.Value, cancellationToken);

        if (createDepartmentResult.IsFailure)
        {
            _logger.LogError("Failed to add location: {ErrorMessage}", createDepartmentResult.Error);
            return createDepartmentResult.Error.ToErrors();
        }

        _logger.LogInformation("Added Department: {DepartmentId}", createDepartmentResult.Value);

        return createDepartmentResult.Value.Value;
    }
}