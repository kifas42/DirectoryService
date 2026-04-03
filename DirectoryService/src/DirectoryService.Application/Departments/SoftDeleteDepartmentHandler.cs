using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record DeleteDepartmentCommand(Guid DepartmentId) : ICommand;

public class SoftDeleteDepartmentHandler : ICommandHandler<DeleteDepartmentCommand>
{
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IPositionRepository _positionRepository;

    public SoftDeleteDepartmentHandler(
        ILogger<SoftDeleteDepartmentHandler> logger,
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        ILocationRepository locationRepository,
        IPositionRepository positionRepository)
    {
        _logger = logger;
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _locationRepository = locationRepository;
        _positionRepository = positionRepository;
    }

    public async Task<UnitResult<Error>> Handle(
        DeleteDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var departmentId = new DepartmentId(command.DepartmentId);

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        // открыли транзакцию
        using var transactionScope = transactionScopeResult.Value;

        var departmentResult = await _departmentRepository.GetBy(d => d.Id == departmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            _logger.LogError(
                "Get department {departmentId} failed: {Error}",
                departmentId.Value,
                departmentResult.Error);
            transactionScope.Rollback();
            return departmentResult.Error;
        }

        // Уже удаленный
        if (!departmentResult.Value.IsActive)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to delete department {Id}: Already deleted", departmentId.Value);
            return Error.Conflict("already.deleted", "Failed to delete department: Already deleted");
        }

        var updateResult = await _departmentRepository.SoftDeleteWithUpdatePath(
            departmentResult.Value,
            $"deleted-{departmentResult.Value.Identifier.Value}",
            cancellationToken);

        if (updateResult.IsFailure)
        {
            _logger.LogError("Update department {departmentId} failed: {Error}", departmentId.Value,
                updateResult.Error);
            transactionScope.Rollback();
            return updateResult.Error;
        }

        var updateLocationsResult = await _locationRepository.SoftDeleteOrphans(departmentId, cancellationToken);

        if (updateLocationsResult.IsFailure)
        {
            _logger.LogError("Update locations failed: {Error}", updateLocationsResult.Error);
            transactionScope.Rollback();
            return updateLocationsResult.Error;
        }

        var updatePositionsResult = await _positionRepository.SoftDeleteOrphans(departmentId, cancellationToken);

        if (updatePositionsResult.IsFailure)
        {
            _logger.LogError("Update positions failed: {Error}", updatePositionsResult.Error);
            transactionScope.Rollback();
            return updatePositionsResult.Error;
        }

        var result = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to SaveChanges for {DepartmentId}: {Error}", departmentId.Value, result.Error);
            transactionScope.Rollback();
            return Error.Failure("db.failure", "Failed to SaveChanges");
        }


        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error;

        return UnitResult.Success<Error>();
    }
}