using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Shared;

namespace DirectoryService.Application.Departments;

public record GetRootDepartmentsQuery(RootDepartmentsRequest Request) : IQuery;

public class GetRootDepartmentsHandler : IQueryHandler<RootDepartmentsResponse, GetRootDepartmentsQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetRootDepartmentsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<RootDepartmentsResponse, Error>> Handle(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        string sql =
            """
            WITH roots AS (
                SELECT d.id,
                       d.parent_id,
                       d.identifier,
                       d.path,
                       d.depth,
                       d.is_active,
                       d.created_at,
                       d.updated_at
                FROM departments d
                WHERE d.parent_id IS NULL
                ORDER BY d.created_at
                OFFSET @offset LIMIT @root_limit
            )
            SELECT *, (EXISTS(SELECT 1 FROM departments WHERE parent_id = roots.id OFFSET @child_limit LIMIT 1)) AS has_more_children
            FROM roots

            UNION ALL

            SELECT c.*,
                   (EXISTS(SELECT 1 FROM departments WHERE parent_id = c.id)) AS has_more_children
            FROM roots r CROSS JOIN LATERAL (
                SELECT d.id,
                       d.parent_id,
                       d.identifier,
                       d.path,
                       d.depth,
                       d.is_active,
                       d.created_at,
                       d.updated_at
                FROM departments d
                WHERE d.parent_id = r.id AND d.is_active = true
                ORDER BY d.created_at
                LIMIT @child_limit
                ) c;
            """;

        var departmentRaws = (await connection.QueryAsync<DepartmentDto>(
                sql,
                param: new
                {
                    offset = (query.Request.Page - 1) * query.Request.Size,
                    root_limit = query.Request.Size,
                    child_limit = query.Request.Prefetch,
                })
            ).ToList();

        var departmentsDict = departmentRaws.ToDictionary(x => x.Id);
        var roots = new List<DepartmentDto>();

        foreach (var row in departmentRaws)
        {
            if (row.ParentId.HasValue && departmentsDict.TryGetValue(row.ParentId.Value, out var parent))
            {
                parent.Children.Add(departmentsDict[row.Id]);
            }
            else
            {
                roots.Add(departmentsDict[row.Id]);
            }
        }

        return new RootDepartmentsResponse(roots);
    }
}