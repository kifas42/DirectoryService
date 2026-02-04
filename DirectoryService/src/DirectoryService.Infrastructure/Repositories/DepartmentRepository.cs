using CSharpFunctionalExtensions;
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

    public Result<Department, Error> GetById(DepartmentId departmentId)
    {
        try
        {
            Department? department = _dbContext.Departments.SingleOrDefault(d => d.Id == departmentId);

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

    public bool IsAllExistAndActive(IEnumerable<DepartmentId> departmentIds)
    {
        return _dbContext.Departments
            .Count(d => departmentIds.Contains(d.Id) && d.IsActive) == departmentIds.Count();
    }

    public async Task<UnitResult<Error>> DeleteLocationsAsync(DepartmentId departmentId, CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentLocations.Where(d => d.DepartmentId == departmentId).ExecuteDeleteAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}