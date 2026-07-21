using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record RootDepartmentsCacheKey
{
    public RootDepartmentsCacheKey(RootDepartmentsRequest request) =>
        Value =
            $"root_departments_pg:{request.Page?.ToString() ?? "null"}_sz:{request.Size?.ToString() ?? "null"}_pf:{request.Prefetch?.ToString() ?? "null"}";

    public string Value { get; }
}

public record GetRootDepartmentsQuery(RootDepartmentsRequest Request) : IQuery;

public class GetRootDepartmentsHandler : IQueryHandler<DepartmentsResponse, GetRootDepartmentsQuery>
{
    private readonly HybridCache _cache;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<GetRootDepartmentsHandler> _logger;

    public GetRootDepartmentsHandler(IDbConnectionFactory connectionFactory, HybridCache cache,
        ILogger<GetRootDepartmentsHandler> logger)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<DepartmentsResponse, Error>> Handle(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        RootDepartmentsCacheKey key = new(query.Request);
        DepartmentsResponse departmentsResponse = await _cache.GetOrCreateAsync<DepartmentsResponse>(
            key.Value,
            ct => GetDepartmentsFromDataBase(query, ct),
            tags: [CacheConstants.DEPARTMENTS_TAG],
            cancellationToken: cancellationToken);

        return departmentsResponse;
    }

    private async ValueTask<DepartmentsResponse> GetDepartmentsFromDataBase(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        _logger.LogDebug("Cache miss - go to database");
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

        List<DepartmentDto> departmentRaws = (await connection.QueryAsync<DepartmentDto>(
                sql,
                new
                {
                    offset = (query.Request.Page - 1) * query.Request.Size,
                    root_limit = query.Request.Size,
                    child_limit = query.Request.Prefetch,
                })
            ).ToList();

        Dictionary<Guid, DepartmentDto> departmentsDict = departmentRaws.ToDictionary(x => x.Id);
        List<DepartmentDto> roots = new();

        foreach (DepartmentDto row in departmentRaws)
        {
            if (row.ParentId.HasValue && departmentsDict.TryGetValue(row.ParentId.Value, out DepartmentDto? parent))
            {
                parent.Children.Add(departmentsDict[row.Id]);
            }
            else
            {
                roots.Add(departmentsDict[row.Id]);
            }
        }

        return new DepartmentsResponse(roots, roots.Count);
    }
}