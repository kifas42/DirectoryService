using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace DirectoryService.Application.Departments;

public record GetTopDepartmentsQuery(int Count) : IQuery;

public class GetTopDepartmentsHandler : IQueryHandler<TopDepartmentsResponse, GetTopDepartmentsQuery>
{
    private readonly HybridCache _cache;
    private readonly IDbConnectionFactory _connectionFactory;

    public GetTopDepartmentsHandler(IDbConnectionFactory connectionFactory, HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<Result<TopDepartmentsResponse, Error>> Handle(
        GetTopDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        TopDepartmentsResponse topDepartmentsResponse = await _cache.GetOrCreateAsync<TopDepartmentsResponse>(
            $"top_departments:{query.Count}",
            ct => GetTopDepartmentsFromDataBase(query, ct),
            tags: [CacheConstants.DEPARTMENTS_TAG, CacheConstants.TOP_DEPARTMENTS_TAG],
            cancellationToken: cancellationToken);

        return topDepartmentsResponse;
    }

    private async ValueTask<TopDepartmentsResponse> GetTopDepartmentsFromDataBase(
        GetTopDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        string sql =
            """
            SELECT
                d.name,
                d.path,
                COUNT(dp.position_id)::int AS pos_count
            FROM departments d
            JOIN department_position dp ON d.id = dp.department_id
            GROUP BY d.id, d.name, d.path
            ORDER BY pos_count DESC, d.name
            LIMIT @count
            """;
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        List<TopDepartmentDto> topDepartments = (await connection.QueryAsync<TopDepartmentDto, int, TopDepartmentDto>(
                sql,
                param: new { count = query.Count },
                splitOn: "pos_count",
                map: (d, c) =>
                    d with { PositionsCount = c }))
            .ToList();

        return new TopDepartmentsResponse(topDepartments, topDepartments.Count);
    }
}