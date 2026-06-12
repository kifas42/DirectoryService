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
            return Error.Validation(
                SharedErrorCodes.Validation.InvalidRequest,
                "Некорректный формат запроса. Проверьте переданные данные.");
        }

        ValidationResult validationResult =
            await _validator.ValidateAsync(command.DepartmentRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var identifierResult = Identifier.Create(command.DepartmentRequest.Identifier);
        if (identifierResult.IsFailure)
        {
            return identifierResult.Error;
        }

        Department? parent = null;
        short depth = 0;

        if (command.DepartmentRequest.ParentId.HasValue)
        {
            var parentId = new DepartmentId(command.DepartmentRequest.ParentId.Value);
            var parentResult = await _departmentRepository.GetByIdIsActive(parentId, cancellationToken);

            if (parentResult.IsFailure)
            {
                _logger.LogWarning(
                    "Parent department not found or inactive: {ParentId}, Reason: {@Error}",
                    parentId.Value,
                    parentResult.Error);

                return parentResult.Error;
            }

            parent = parentResult.Value;
            depth = (short)(parent.Depth + 1);
        }

        var departmentId = DepartmentId.New();
        var departmentLocations = command.DepartmentRequest.LocationIds
            .Select(locationId => new DepartmentLocation(Guid.NewGuid(), departmentId, new LocationId(locationId)));

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
            _logger.LogError("Failed to create department entity: {@Error}", departmentResult.Error);
            return departmentResult.Error;
        }

        var createDepartmentResult = await _departmentRepository.AddAsync(departmentResult.Value, cancellationToken);

        if (createDepartmentResult.IsFailure)
        {
            _logger.LogError("Failed to add department to database");
            return createDepartmentResult.Error;
        }

        _logger.LogInformation("Successfully added Department: {DepartmentId}", createDepartmentResult.Value.Value);

        return createDepartmentResult.Value.Value;
    }
}