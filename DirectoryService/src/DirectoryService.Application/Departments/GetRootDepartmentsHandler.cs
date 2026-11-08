using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments;

public record RootDepartmentsCacheKey
{
    public RootDepartmentsCacheKey(RootDepartmentsRequest request) =>
        Value =
            $"root_departments_v2_pg:{request.Page?.ToString() ?? "null"}_sz:{request.Size?.ToString() ?? "null"}_pf:{request.Prefetch?.ToString() ?? "null"}";

    public string Value { get; }
}

public record GetRootDepartmentsQuery(RootDepartmentsRequest Request) : IQuery;

public class GetRootDepartmentsHandler : IQueryHandler<PaginationResponse<GetDepartmentDto>, GetRootDepartmentsQuery>
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

    public async Task<Result<PaginationResponse<GetDepartmentDto>, Error>> Handle(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        int page = query.Request.Page ?? 1;
        int pageSize = query.Request.Size ?? 20;
        int prefetch = query.Request.Prefetch ?? 3;

        if (page < 1)
        {
            return Error.Validation(
                SharedErrorCodes.Validation.InvalidRequest,
                "Номер страницы не может быть меньше 1",
                "page");
        }

        if (pageSize is < 1 or > 100)
        {
            return Error.Validation(
                SharedErrorCodes.Validation.InvalidRequest,
                "Размер страницы должен быть от 1 до 100",
                "size");
        }

        if (prefetch < 0)
        {
            return Error.Validation(
                SharedErrorCodes.Validation.InvalidRequest,
                "Количество подгружаемых детей не может быть отрицательным",
                "prefetch");
        }

        RootDepartmentsRequest request = new(page, pageSize, prefetch);
        RootDepartmentsCacheKey key = new(request);
        PaginationResponse<GetDepartmentDto> departmentsResponse =
            await _cache.GetOrCreateAsync<PaginationResponse<GetDepartmentDto>>(
                key.Value,
                ct => GetDepartmentsFromDataBase(request, ct),
                tags: [CacheConstants.DEPARTMENTS_TAG],
                cancellationToken: cancellationToken);

        return departmentsResponse;
    }

    private async ValueTask<PaginationResponse<GetDepartmentDto>> GetDepartmentsFromDataBase(
        RootDepartmentsRequest request,
        CancellationToken cancellationToken)
    {
        int page = request.Page ?? 1;
        int pageSize = request.Size ?? 20;
        int prefetch = request.Prefetch ?? 3;

        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        const string countSql =
            """
            SELECT COUNT(*)
            FROM departments d
            WHERE d.parent_id IS NULL;
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            countSql,
            cancellationToken: cancellationToken));

        _logger.LogDebug("Cache miss - go to database");
        string sql =
            """
            WITH roots AS (
                SELECT d.id,
                       d.parent_id,
                       d.name,
                       d.identifier,
                       d.path,
                       d.depth,
                       d.is_active,
                       d.created_at,
                       d.updated_at,
                       d.deleted_at
                FROM departments d
                WHERE d.parent_id IS NULL
                ORDER BY d.created_at, d.id
                OFFSET @offset LIMIT @root_limit
            )
            SELECT *, (EXISTS(SELECT 1 FROM departments WHERE parent_id = roots.id AND is_active = true OFFSET @child_limit LIMIT 1)) AS has_more_children
            FROM roots

            UNION ALL

            SELECT c.*,
                   (EXISTS(SELECT 1 FROM departments WHERE parent_id = c.id)) AS has_more_children
            FROM roots r CROSS JOIN LATERAL (
                SELECT d.id,
                       d.parent_id,
                       d.name,
                       d.identifier,
                       d.path,
                       d.depth,
                       d.is_active,
                       d.created_at,
                       d.updated_at,
                       d.deleted_at
                FROM departments d
                WHERE d.parent_id = r.id AND d.is_active = true
                ORDER BY d.created_at, d.id
                LIMIT @child_limit
                ) c;
            """;

        List<GetDepartmentDto> departmentRaws = (await connection.QueryAsync<GetDepartmentDto>(
                new CommandDefinition(
                    sql,
                    new { offset = ((long)page - 1) * pageSize, root_limit = pageSize, child_limit = prefetch, },
                    cancellationToken: cancellationToken))
            ).ToList();

        Dictionary<Guid, GetDepartmentDto> departmentsDict = departmentRaws.ToDictionary(x => x.Id);
        List<GetDepartmentDto> departments = new();

        foreach (GetDepartmentDto row in departmentRaws)
        {
            if (row.ParentId.HasValue && departmentsDict.TryGetValue(row.ParentId.Value, out GetDepartmentDto? parent))
            {
                parent.Children.Add(departmentsDict[row.Id]);
            }
            else
            {
                departments.Add(departmentsDict[row.Id]);
            }
        }

        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginationResponse<GetDepartmentDto>(departments, totalCount, page, pageSize, totalPages);
    }
}