using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record UpdateParentCommand(Guid DepartmentId, UpdateParentRequest Request) : ICommand;

public class UpdateParentHandler : ICommandHandler<int, UpdateParentCommand>
{
    private readonly HybridCache _cache;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<UpdateParentHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public UpdateParentHandler(
        ILogger<UpdateParentHandler> logger,
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        HybridCache cache)
    {
        _logger = logger;
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _cache = cache;
    }

    public async Task<Result<int, Error>> Handle(
        UpdateParentCommand command,
        CancellationToken cancellationToken = default)
    {
        Result<ITransactionScope, Error> transactionScopeResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using ITransactionScope? transactionScope = transactionScopeResult.Value;
        DepartmentId departmentId = new(command.DepartmentId);

        bool isParentNotNull = command.Request.ParentId.HasValue;

        DepartmentId? parentId =
            isParentNotNull ? new DepartmentId(command.Request.ParentId!.Value) : null;

        UnitResult<Error> lockDep = await _departmentRepository.LockDepartmentsById(departmentId, cancellationToken);
        if (lockDep.IsFailure)
        {
            _logger.LogWarning("Fail to lock department {departmentId}", departmentId.Value);
            transactionScope.Rollback();
            return lockDep.Error;
        }

        // редактируемый департамент активен
        Result<Department, Error> departmentResult =
            await _departmentRepository.GetByIdIsActive(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error;
        }

        // новый родитель - не мы
        if (parentId == departmentId)
        {
            transactionScope.Rollback();
            return Error.Conflict(
                DomainErrorCodes.Department.SelfReferenceParent,
                "Департамент не может быть родителем самого себя",
                "parentId");
        }

        Department? newParent = null;
        if (isParentNotNull)
        {
            // новый родитель существует
            Result<Department, Error> parentResult =
                await _departmentRepository.GetByIdIsActive(parentId!, cancellationToken);
            if (parentResult.IsFailure)
            {
                transactionScope.Rollback();
                return parentResult.Error;
            }

            newParent = parentResult.Value;
            if (newParent.Path.Value.StartsWith(departmentResult.Value.Path.Value + "."))
            {
                transactionScope.Rollback();
                return Error.Conflict(
                    DomainErrorCodes.Department.CyclicReference,
                    "Нельзя переместить департамент в его собственного потомка",
                    "parentId");
            }
        }

        UnitResult<Error> updateResult =
            await _departmentRepository.UpdateDepartmentDescendants(departmentResult.Value, newParent,
                cancellationToken);

        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateResult.Error;
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

        await _cache.RemoveByTagAsync(CacheConstants.DEPARTMENTS_TAG, cancellationToken);

        return 0;
    }
}