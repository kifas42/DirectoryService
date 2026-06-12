using System.Data.Common;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(ILogger<DepartmentRepository> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Result<DepartmentId, Error>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        _dbContext.Departments.Add(department);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return department.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            return pgEx switch
            {
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                    when pgEx.ConstraintName.Contains(
                        IndexConstants.DEPARTMENT_IDENTIFIER,
                        StringComparison.OrdinalIgnoreCase) =>
                    Error.Conflict(
                        DomainErrorCodes.Department.IdentifierConflict,
                        "Департамент с таким идентификатором уже существует",
                        "identifier"),
                { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: not null }
                    when pgEx.ConstraintName.Equals(
                        "FK_department_location_locations_location_id",
                        StringComparison.OrdinalIgnoreCase) =>
                    Error.Conflict(
                        DomainErrorCodes.Department.InvalidLocationReference,
                        "Выбранная локация не существует или была удалена",
                        "locationId"),
                _ => HandleUnexpectedDbError(ex)
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled while adding department");
            return Error.Failure(SharedErrorCodes.System.OperationCanceled, "Операция была прервана");
        }
        catch (Exception ex)
        {
            return HandleUnexpectedDbError(ex);
        }
    }

    public async Task<Result<Department, Error>> GetByIdIsActive(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            Department? department = await _dbContext.Departments.SingleOrDefaultAsync(
                d => d.Id == departmentId && d.IsActive,
                cancellationToken);

            if (department is null)
            {
                return Error.NotFound(
                    DomainErrorCodes.Department.NotFound,
                    $"Департамент с ID '{departmentId.Value}' не найден или неактивен");
            }

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активного департамента с ID {DepartmentId}", departmentId.Value);
            return Error.Failure(
                SharedErrorCodes.System.Database.OperationFailed,
                "Произошла внутренняя ошибка базы данных при получении данных");
        }
    }

    public async Task<UnitResult<Error>> SoftDeleteWithUpdatePath(
        Department department,
        string newIdentifier,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE departments
                           SET path =
                                   subpath(path, 0, nlevel(@oldPath::ltree) - 1)
                                       ||
                                   @newIdentifier::ltree
                                       ||
                                   CASE
                                       WHEN path != @oldPath::ltree
                                           THEN subpath(path, nlevel(@oldPath::ltree))
                                       ELSE ''::ltree
                                       END,
                               identifier = CASE
                                                WHEN path = @oldPath::ltree
                                                    THEN @newIdentifier
                                                ELSE identifier END,
                               is_active = CASE 
                                               WHEN path = @oldPath::ltree
                                                   THEN FALSE
                                               ELSE is_active END,
                               deleted_at = CASE
                                                WHEN path = @oldPath::ltree
                                                    THEN NOW()
                                                ELSE deleted_at END,
                               updated_at = NOW()
                           WHERE path <@ @oldPath::ltree;
                           """;

        try
        {
            DbConnection dbConn = _dbContext.Database.GetDbConnection();

            int updated = await dbConn.ExecuteAsync(
                sql,
                new { oldPath = department.Path.Value, newIdentifier },
                commandTimeout: 30);

            _logger.LogInformation(
                "Successfully updated path for {Count} departments (Target DepartmentId: {DepartmentId})",
                updated,
                department.Id.Value);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update department path for DepartmentId {DepartmentId}",
                department.Id.Value);

            return Error.Failure(
                DomainErrorCodes.Department.PathUpdateFailed,
                "Не удалось обновить данные департамента. Пожалуйста, попробуйте позже.");
        }
    }

    public async Task<UnitResult<Error>> LockDepartmentsById(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 SELECT * FROM departments 
                 WHERE path <@ (SELECT path FROM departments WHERE id = {departmentId.Value} AND is_active = true) 
                 FOR UPDATE
                 """,
                cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire row lock for departments under DepartmentId {DepartmentId}",
                departmentId.Value);

            return Error.Failure(
                DomainErrorCodes.Department.LockFailed,
                "Не удалось заблокировать данные для операции. Возможно, система перегружена, попробуйте через несколько секунд.");
        }
    }

    public async Task<Result<Department, Error>> GetBy(
        Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken)
    {
        Department? department =
            await _dbContext.Departments.FirstOrDefaultAsync(predicate, cancellationToken);

        if (department is null)
        {
            return Error.NotFound(DomainErrorCodes.Department.NotFound, "Не найдено подразделение по заданным фильтрам");
        }

        return department;
    }

    public async Task<bool> IsAllExistAndActive(IEnumerable<DepartmentId> departmentIds) =>
        await _dbContext.Departments
            .CountAsync(d => departmentIds.Contains(d.Id) && d.IsActive) == departmentIds.Count();

    public async Task<UnitResult<Error>> DeleteLocationsAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentLocations.Where(d => d.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> UpdateDepartmentDescendants(
        Department root,
        Department? newParent,
        CancellationToken cancellationToken)
    {

        const string sql = """
                           UPDATE departments
                           SET 
                               path = CASE 
                                   WHEN @newParentId IS NULL 
                                   THEN subpath(path, nlevel(@oldPath::ltree) - 1)
                                   ELSE @newParentPath::ltree || subpath(path, nlevel(@oldPath::ltree) - 1)
                               END,
                               depth = CASE 
                                   WHEN @newParentId IS NULL 
                                   THEN nlevel(path) - nlevel(@oldPath::ltree)
                                   ELSE nlevel(@newParentPath::ltree) + (nlevel(path) - nlevel(@oldPath::ltree))
                               END,
                               parent_id = CASE 
                                   WHEN id = @departmentId
                                   THEN @newParentId 
                                   ELSE parent_id 
                               END,
                               updated_at = NOW()
                           WHERE path <@ @oldPath::ltree;
                           """;

        try
        {
            DbConnection dbConn = _dbContext.Database.GetDbConnection();

            int updated = await dbConn.ExecuteAsync(
                sql,
                new
                {
                    departmentId = root.Id.Value,
                    newParentId = newParent?.Id.Value,
                    oldPath = root.Path.Value,
                    newParentPath = newParent?.Path.Value,
                },
                commandTimeout: 30);

            _logger.LogInformation(
                "Successfully updated hierarchy for {Count} departments (Root: {RootId}, NewParent: {NewParentId})",
                updated,
                root.Id.Value,
                newParent?.Id.Value);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update departments hierarchy for Root {RootId}", root.Id.Value);

            return Error.Failure(
                DomainErrorCodes.Department.HierarchyUpdateFailed,
                "Не удалось обновить иерархию департаментов. Пожалуйста, попробуйте позже.");
        }
    }

    private Error HandleUnexpectedDbError(Exception ex)
    {
        _logger.LogError(ex, "Unexpected database error while adding department");

        return Error.Failure(
            SharedErrorCodes.System.Database.OperationFailed,
            "Произошла внутренняя ошибка. Пожалуйста, попробуйте позже.");
    }
}