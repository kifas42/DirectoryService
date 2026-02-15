using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ILogger<DepartmentRepository> _logger;
    private readonly ApplicationDbContext _dbContext;

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
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } when
                    pgEx.ConstraintName.Contains(
                        IndexConstants.DEPARTMENT_IDENTIFIER,
                        StringComparison.CurrentCultureIgnoreCase) =>
                    Error.Conflict("unique.conflict", "Identifier conflict"),
                { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: not null } when
                    pgEx.ConstraintName == "FK_department_location_locations_location_id" =>
                    Error.Conflict("fk.conflict", "Specified location does not exist"),
                _ => Error.Failure(null, "database error. check logs")
            };
        }
        catch (OperationCanceledException ex)
        {
            return Error.Failure(null, "OperationCanceled");
        }
        catch (Exception ex)
        {
            _logger.LogError("Add Error: {Message}", ex.Message);
            return Error.Failure(null, "database error. check logs");
        }
    }

    public async Task<Result<Department, Error>> GetByIdIsActive(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            Department? department =
                await _dbContext.Departments.SingleOrDefaultAsync(
                    d => d.Id == departmentId && d.IsActive,
                    cancellationToken);

            if (department is null)
            {
                return Error.NotFound("get.department", "Department not found or not single", departmentId.Value);
            }

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError("GetById Error: {Message}", ex.Message);
            return Error.Failure(null, "database error. check logs");
        }
    }

    public async Task<UnitResult<Error>> LockDepartmentsById(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $@"SELECT * FROM departments 
                   WHERE path <@ (SELECT path FROM departments WHERE id = {departmentId.Value} AND is_active = true) 
                   FOR UPDATE",
                cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Lock Error: {Message}", ex.Message);
            return Error.Failure("data.is.locked", "Lock departments error");
        }
    }

    public async Task<bool> IsAllExistAndActive(IEnumerable<DepartmentId> departmentIds)
    {
        return await _dbContext.Departments
            .CountAsync(d => departmentIds.Contains(d.Id) && d.IsActive) == departmentIds.Count();
    }

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
        // const string sql = """
        //                     SELECT * FROM departments
        //                     WHERE path <@ @rootPath::ltree
        //                     ORDER BY depth
        //                    """;
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
            var dbConn = _dbContext.Database.GetDbConnection();

            // var departmentRaws = (await dbConn.QueryAsync<DepartmentDto>(sql, new { rootPath })).ToList();
            int updated = await dbConn.ExecuteAsync(
                sql,
                new
                {
                    departmentId = root.Id.Value,
                    newParentId = newParent?.Id.Value,
                    oldPath = root.Path.Value,
                    newParentPath = newParent?.Path.Value,
                });

            _logger.LogInformation("Updated {Count} departments", updated);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to update departments hierarchy: {Message}", e.Message);
            return Error.Failure("update.departments", "Failed to update departments hierarchy");
        }
    }
}