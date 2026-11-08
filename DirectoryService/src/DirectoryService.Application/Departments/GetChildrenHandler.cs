using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using Shared;

namespace DirectoryService.Application.Departments;

public record GetChildDepartmentsQuery(Guid Id, ChildDepartmentsRequest Request) : IQuery;

public class GetChildrenHandler : IQueryHandler<PaginationResponse<GetDepartmentDto>, GetChildDepartmentsQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetChildrenHandler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<Result<PaginationResponse<GetDepartmentDto>, Error>> Handle(
        GetChildDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        int page = query.Request.Page ?? 1;
        int pageSize = query.Request.Size ?? 20;

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

        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string countSql =
            """
            SELECT COUNT(*)
            FROM departments d
            WHERE d.parent_id = @id
              AND d.is_active = true;
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            countSql,
            new { id = query.Id },
            cancellationToken: cancellationToken));

        const string sql =
            """
            SELECT
                d.id,
                d.parent_id,
                d.name,
                d.identifier,
                d.path,
                d.depth,
                d.is_active,
                d.created_at,
                d.updated_at,
                d.deleted_at,
                EXISTS(
                    SELECT 1
                    FROM departments child
                    WHERE child.parent_id = d.id) AS has_more_children
            FROM departments d
            WHERE d.parent_id = @id
              AND d.is_active = true
            ORDER BY d.created_at, d.id
            OFFSET @offset LIMIT @limit;
            """;

        List<GetDepartmentDto> departments =
        [
            .. await connection.QueryAsync<GetDepartmentDto>(
                new CommandDefinition(
                    sql,
                    new { id = query.Id, offset = ((long)page - 1) * pageSize, limit = pageSize, },
                    cancellationToken: cancellationToken))
        ];

        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginationResponse<GetDepartmentDto>(departments, totalCount, page, pageSize, totalPages);
    }
}