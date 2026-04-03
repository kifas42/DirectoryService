using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Shared;

namespace DirectoryService.Application.Departments;

public record GetChildDepartmentsQuery(Guid Id, ChildDepartmentsRequest Request) : IQuery;

public class GetChildrenHandler : IQueryHandler<DepartmentsResponse, GetChildDepartmentsQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetChildrenHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<DepartmentsResponse, Error>> Handle(
        GetChildDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        string sql =
            """
            SELECT
                d.id,
                d.parent_id,
                d.identifier,
                d.path,
                d.depth,
                d.is_active,
                d.created_at,
                d.updated_at,
                EXISTS(
                    SELECT 1
                    FROM departments child
                    WHERE child.parent_id = d.id) AS has_more_children
            FROM departments d
            WHERE d.parent_id = @id
              AND d.is_active = true
            ORDER BY d.created_at
            OFFSET @offset LIMIT @limit;
            """;

        var departmentRaws = (await connection.QueryAsync<DepartmentDto>(
                sql,
                param: new
                {
                    id = query.Id,
                    offset = (query.Request.Page - 1) * query.Request.Size,
                    limit = query.Request.Size,
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

        return new DepartmentsResponse(roots);
    }
}