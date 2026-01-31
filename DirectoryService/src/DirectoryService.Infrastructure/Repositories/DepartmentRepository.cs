using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Domain.Department;
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
            _logger.LogError("GetById Error: {Message}", ex.Message);
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
}

/*
    public Result<Department, Error> GetByIdWithParents(DepartmentId departmentId)
    {
        var rawDepartments = _dbContext.Departments
            .FromSqlInterpolated($@"
            WITH RECURSIVE dept_ancestors AS (
                SELECT * FROM departments WHERE id = {departmentId.Value}
                UNION ALL
                SELECT d.* FROM departments d
                INNER JOIN dept_ancestors da ON d.id = da.parent_id
            )
            SELECT * FROM dept_ancestors")
            .ToList();

        if (!rawDepartments.Any())
        {
            return Error.NotFound("get.department", "Department not found", departmentId.Value);
        }

        var dict = rawDepartments.ToDictionary(d => d.Id);

        // Находим исходный отдел
        var targetDept = dict[departmentId];

        return targetDept;
    }

    public Result<Department, Error> GetHierarchyLtree(string rootPath)
    {
        var rawDepartments = _dbContext.Departments
            .FromSqlInterpolated($"""
                                  SELECT id,
                                      parent_id,
                                      name,
                                      identifier,
                                      path,
                                      depth,
                                      is_active,
                                      created_at,
                                      updated_at
                                  FROM departments
                                  WHERE path <@ @rootPath::ltree
                                  ORDER BY depth
                                  """)
            .ToList();

        if (!rawDepartments.Any())
        {
            return Error.NotFound("get.department", "Ltree hierarchy Department not found", null);
        }

        var dict = rawDepartments.ToDictionary(d => d.Id);
        var roots = new List<Department>();
        foreach (var department in rawDepartments)
        {
            if (department.ParentId is not null && dict.TryGetValue(department.ParentId, out var parent))
            {
                parent.
                    // перевернуть логику. добавлять родителей к детям
            }
        }

        return targetDept;
    }
}*/