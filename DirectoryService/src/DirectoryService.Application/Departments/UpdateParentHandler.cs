using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record UpdateParentCommand(Guid DepartmentId, UpdateParentRequest Request) : ICommand;

public class UpdateParentHandler : ICommandHandler<int, UpdateParentCommand>
{
    private readonly ILogger<UpdateParentHandler> _logger;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;

    public UpdateParentHandler(
        ILogger<UpdateParentHandler> logger,
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<int, Errors>> Handle(
        UpdateParentCommand command,
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;
        var departmentId = new DepartmentId(command.DepartmentId);

        bool isParentNotNull = command.Request.ParentId.HasValue;

        DepartmentId? parentId =
            isParentNotNull ? new DepartmentId(command.Request.ParentId!.Value) : null;

        var lockDep = await _departmentRepository.LockDepartmentsById(departmentId, cancellationToken);
        if (lockDep.IsFailure)
        {
            _logger.LogWarning("Fail to lock department {departmentId}", departmentId.Value);
            return lockDep.Error.ToErrors();
        }

        // редактируемый департамент активен
        var departmentResult = await _departmentRepository.GetByIdIsActive(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            errors.Add(departmentResult.Error);
        }

        // новый родитель - не мы
        if (parentId == departmentId)
        {
            errors.Add(Error.Conflict("invalid.parent", "Не может быть сам себе родителем"));
        }

        Department? newParent = null;
        if (isParentNotNull)
        {
            // новый родитель существует
            var parentResult = await _departmentRepository.GetByIdIsActive(parentId!, cancellationToken);
            if (parentResult.IsFailure)
            {
                errors.Add(parentResult.Error);
            }

            newParent = parentResult.Value;
            if (newParent.Path.Value.StartsWith(departmentResult.Value.Path.Value + "."))
                errors.Add(Error.Failure("invalid.parent", "Нельзя переместить в своего потомка"));
        }

        if (errors.Count != 0)
        {
            transactionScope.Rollback();
            return new Errors(errors);
        }

        var updateResult =
            await _departmentRepository.UpdateDepartmentDescendants(departmentResult.Value, newParent,
                cancellationToken);

        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateResult.Error.ToErrors();
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

        return 0;
    }
}