using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace DirectoryService.Application.Positions;

public record CreatePositionCommand(CreatePositionRequest PositionRequest) : ICommand;

public sealed class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly HybridCache _cache;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IValidator<CreatePositionRequest> _validator;

    public CreatePositionHandler(
        IPositionRepository positionRepository,
        IValidator<CreatePositionRequest> validator,
        IDepartmentRepository departmentRepository,
        HybridCache cache)
    {
        _positionRepository = positionRepository;
        _validator = validator;
        _departmentRepository = departmentRepository;
        _cache = cache;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command.PositionRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        List<DepartmentId> departmentIds = command.PositionRequest.DepartmentIds
            .Select(g => new DepartmentId(g)).ToList();

        if (!await _departmentRepository.IsAllExistAndActive(departmentIds))
        {
            return Error.NotFound(
                DomainErrorCodes.Department.NotFound,
                "Привязанные подразделения не существуют или не активны",
                "departmentIds");
        }

        PositionId positionId = PositionId.New();

        IEnumerable<DepartmentPosition> departmentPositions = departmentIds
            .Select(g => new DepartmentPosition(Guid.NewGuid(), g, positionId));

        Result<Position, Error> positionResult = Position.Create(
            positionId,
            command.PositionRequest.Name,
            command.PositionRequest.Description,
            departmentPositions);

        if (positionResult.IsFailure)
        {
            return positionResult.Error;
        }

        Result<PositionId, Error> createPositionResult =
            await _positionRepository.AddAsync(positionResult.Value, cancellationToken);

        if (createPositionResult.IsFailure)
        {
            return createPositionResult.Error;
        }

        await _cache.RemoveByTagAsync(CacheConstants.TOP_DEPARTMENTS_TAG, cancellationToken);

        return createPositionResult.Value.Value;
    }
}