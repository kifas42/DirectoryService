using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Positions;

public record CreatePositionCommand(CreatePositionRequest PositionRequest) : ICommand;

public sealed class CreatePositionHandler : ICommandHandler<PositionId, CreatePositionCommand>
{
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<CreatePositionRequest> _validator;

    public CreatePositionHandler(
        ILogger<CreatePositionHandler> logger,
        IPositionRepository positionRepository,
        IValidator<CreatePositionRequest> validator,
        IDepartmentRepository departmentRepository)
    {
        _logger = logger;
        _positionRepository = positionRepository;
        _validator = validator;
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<PositionId, Errors>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];
        var validationResult = await _validator.ValidateAsync(command.PositionRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            errors.AddRange(validationResult.ToErrors());
        }

        var departmentIds = command.PositionRequest.DepartmentIds
            .Select(g => new DepartmentId(g)).ToList();

        if (!_departmentRepository.IsAllExistAndActive(departmentIds))
        {
            errors.Add(Error.NotFound("find.active.departments", "Department not found", null));
        }

        if (errors.Count != 0)
        {
            return new Errors(errors);
        }

        var positionId = PositionId.New();

        var departmentPositions = departmentIds
            .Select(g => new DepartmentPosition(g, positionId));

        var positionResult =
            Position.Create(positionId, command.PositionRequest.Name, command.PositionRequest.Description,
                departmentPositions);

        if (positionResult.IsFailure)
        {
            return positionResult.Error.ToErrors();
        }

        positionResult.Value.Activate();

        var createPositionResult = await _positionRepository.AddAsync(positionResult.Value, cancellationToken);

        if (createPositionResult.IsFailure)
        {
            return createPositionResult.Error.ToErrors();
        }


        return createPositionResult.Value;
    }
}