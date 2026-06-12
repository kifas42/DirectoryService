using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record DeleteDepartmentCommand(Guid DepartmentId) : ICommand;

public class SoftDeleteDepartmentHandler : ICommandHandler<DeleteDepartmentCommand>
{
    private readonly HybridCache _cache;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;

    public SoftDeleteDepartmentHandler(
        ILogger<SoftDeleteDepartmentHandler> logger,
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        ILocationRepository locationRepository,
        IPositionRepository positionRepository,
        HybridCache cache)
    {
        _logger = logger;
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _locationRepository = locationRepository;
        _positionRepository = positionRepository;
        _cache = cache;
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

        using var transactionScope = transactionScopeResult.Value;

        var departmentResult = await _departmentRepository.GetBy(d => d.Id == departmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            _logger.LogError(
                "Failed to get department {DepartmentId} for deletion. Details: {@Error}",
                departmentId.Value,
                departmentResult.Error);

            transactionScope.Rollback();
            return departmentResult.Error;
        }

        var department = departmentResult.Value;

        // 3. Проверка: уже удален
        if (!department.IsActive)
        {
            _logger.LogWarning(
                "Attempted to delete already inactive department {DepartmentId} (Identifier: {Identifier})",
                departmentId.Value,
                department.Identifier.Value);

            transactionScope.Rollback();

            return Error.Conflict(
                DomainErrorCodes.Department.AlreadyDeleted,
                "Департамент уже был удален ранее",
                null);
        }

        // 4. Soft delete департамента
        var updateResult = await _departmentRepository.SoftDeleteWithUpdatePath(
            department,
            $"deleted-{department.Identifier.Value}",
            cancellationToken);

        if (updateResult.IsFailure)
        {
            _logger.LogError(
                "Failed to soft-delete department {DepartmentId}. Details: {@Error}",
                departmentId.Value,
                updateResult.Error);
            transactionScope.Rollback();
            return updateResult.Error;
        }

        var updateLocationsResult = await _locationRepository.SoftDeleteOrphans(departmentId, cancellationToken);

        if (updateLocationsResult.IsFailure)
        {
            _logger.LogError(
                "Failed to soft-delete orphan locations for department {DepartmentId}. Details: {@Error}",
                departmentId.Value,
                updateLocationsResult.Error);
            transactionScope.Rollback();
            return updateLocationsResult.Error;
        }

        var updatePositionsResult = await _positionRepository.SoftDeleteOrphans(departmentId, cancellationToken);

        if (updatePositionsResult.IsFailure)
        {
            _logger.LogError(
                "Failed to soft-delete orphan positions for department {DepartmentId}. Details: {@Error}",
                departmentId.Value,
                updatePositionsResult.Error);
            transactionScope.Rollback();
            return updatePositionsResult.Error;
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            _logger.LogError(
                "Failed to SaveChanges for department {DepartmentId}. Details: {@Error}",
                departmentId.Value,
                saveResult.Error);
            transactionScope.Rollback();

            return Error.Failure(
                SharedErrorCodes.System.Database.OperationFailed,
                "Не удалось сохранить изменения в базе данных");
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogError(
                "Failed to commit transaction for department deletion {DepartmentId}. Details: {@Error}",
                departmentId.Value,
                commitResult.Error);
            return commitResult.Error;
        }

        await _cache.RemoveByTagAsync(CacheConstants.DEPARTMENTS_TAG, cancellationToken);

        _logger.LogInformation(
            "Successfully deleted department {DepartmentId} (Identifier: {Identifier}, Name: {Name})",
            departmentId.Value,
            department.Identifier.Value,
            department.Name);

        return UnitResult.Success<Error>();
    }
}