using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Shared;

namespace DirectoryService.Application.Departments;

public record GetTopDepartmentsQuery(int Count) : IQuery;

public class GetTopDepartmentsHandler : IQueryHandler<TopDepartmentsResponse, GetTopDepartmentsQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetTopDepartmentsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<TopDepartmentsResponse, Errors>> Handle(
        GetTopDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var connection = _readDbContext.Connection;

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

        var topDepartments = (await connection.QueryAsync<TopDepartmentDto, int, TopDepartmentDto>(
                sql,
                param: new { count = query.Count },
                splitOn: "pos_count",
                map: (d, c) =>
                    d with { PositionsCount = c }))
            .ToList();

        return new TopDepartmentsResponse(topDepartments, topDepartments.Count);
    }
}