using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Application.Positions;

public record GetPositionQuery(GetPositionRequest Request) : IQuery;

public class GetPositionHandler : IQueryHandler<InfiniteScrollResponse<GetPositionDto>, GetPositionQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetPositionHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<InfiniteScrollResponse<GetPositionDto>, Error>> Handle(
        GetPositionQuery positionQuery,
        CancellationToken cancellationToken)
    {
        string? cursor = positionQuery.Request.Cursor;
        int limit = positionQuery.Request.Limit;

        IQueryable<Position> query = _readDbContext.PositionsRead;

        if (!string.IsNullOrWhiteSpace(positionQuery.Request.Search))
        {
            query = query.Where(l => EF.Functions.Like(l.Name, $"%{positionQuery.Request.Search}%"));
        }

        if (positionQuery.Request.DepartmentIds is not null && positionQuery.Request.DepartmentIds.Length != 0)
        {
            List<DepartmentId> departmentIds = positionQuery.Request.DepartmentIds
                .Select(id => new DepartmentId(id))
                .ToList();

            query = (from l in query
                join dl in _readDbContext.DepartmentPositionsRead on l.Id equals dl.PositionId
                where departmentIds.Contains(dl.DepartmentId)
                select l).Distinct();
        }

        if (positionQuery.Request.IsActive.HasValue)
        {
            query = query.Where(l => l.IsActive == positionQuery.Request.IsActive);
        }

        string sortBy = positionQuery.Request.SortBy?.ToLower() ?? "name";
        bool isAsc = positionQuery.Request.SortOrder?.ToLower() == "asc";

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (sortBy == "date")
            {
                // Если сортируем по дате, парсим курсор в DateTimeOffset или DateTime
                if (DateTimeOffset.TryParse(cursor, out var cursorDate))
                {
                    query = isAsc
                        ? query.Where(l => l.CreatedAt > cursorDate)
                        : query.Where(l => l.CreatedAt < cursorDate);
                }
            }
            else // по умолчанию "name"
            {
                // Если сортируем по имени, сравниваем строки по алфавиту
                query = isAsc
                    ? query.Where(l => string.Compare(l.Name, cursor) > 0)
                    : query.Where(l => string.Compare(l.Name, cursor) < 0);
            }
        }

        if (sortBy == "date")
        {
            query = isAsc ? query.OrderBy(l => l.CreatedAt) : query.OrderByDescending(l => l.CreatedAt);
        }
        else // "name"
        {
            query = isAsc ? query.OrderBy(l => l.Name) : query.OrderByDescending(l => l.Name);
        }


        List<GetPositionDto> positions = await query
            .Select(l => new GetPositionDto
            {
                Id = l.Id.Value,
                Name = l.Name,
                Description = l.Description,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
            })
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;

        if (positions.Count <= limit)
        {
            return new InfiniteScrollResponse<GetPositionDto>(positions, nextCursor);
        }

        // Нашли запасной элемент — значит, дальше тоже есть данные.
        // Удаляем его из выдачи, чтобы не показывать пользователю раньше времени.
        positions.RemoveAt(positions.Count - 1);

        // Берем данные последнего РЕАЛЬНОГО элемента для курсора
        GetPositionDto lastItem = positions.Last();

        nextCursor = sortBy == "date"
            ? lastItem.CreatedAt.ToString("o") // Формат ISO 8601 для дат ("2026-06-19T...")
            : lastItem.Name; // Имя в качестве курсора

        return new InfiniteScrollResponse<GetPositionDto>(positions, nextCursor);
    }
}