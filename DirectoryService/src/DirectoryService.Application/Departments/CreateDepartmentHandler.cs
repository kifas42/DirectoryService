using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record CreateDepartmentCommand(CreateDepartmentRequest? DepartmentRequest) : ICommand;

public sealed class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
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

    public async Task<Result<Guid, Error>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.DepartmentRequest == null)
        {
            return Error.Failure("fail", "invalid request");
        }

        ValidationResult? validationResult =
            await _validator.ValidateAsync(command.DepartmentRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        Result<Identifier, Error> identifierResult = Identifier.Create(command.DepartmentRequest.Identifier);
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error;
        }

        short depth = 0;
        Department? parent = null;
        if (command.DepartmentRequest.ParentId != null)
        {
            DepartmentId parentId = new(command.DepartmentRequest.ParentId.Value);
            Result<Department, Error> parentResult =
                await _departmentRepository.GetByIdIsActive(parentId, cancellationToken);

            if (parentResult.IsFailure)
            {
                _logger.LogError("Parent department not found: {ErrorMessage}", parentResult.Error);
            }

            parent = parentResult.Value;

            depth = (short)(parentResult.Value.Depth + 1);
        }

        DepartmentId departmentId = DepartmentId.New();
        IEnumerable<DepartmentLocation> departmentLocations = command.DepartmentRequest.LocationIds
            .Select(g => new DepartmentLocation(Guid.NewGuid(), departmentId, new LocationId(g)));

        Result<Department, Error> departmentResult = Department.Create(
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
            return departmentResult.Error;
        }

        Result<DepartmentId, Error> createDepartmentResult =
            await _departmentRepository.AddAsync(departmentResult.Value, cancellationToken);

        if (createDepartmentResult.IsFailure)
        {
            _logger.LogError("Failed to add location: {ErrorMessage}", createDepartmentResult.Error);
            return createDepartmentResult.Error;
        }

        _logger.LogInformation("Added Department: {DepartmentId}", createDepartmentResult.Value);

        return createDepartmentResult.Value.Value;
    }
}