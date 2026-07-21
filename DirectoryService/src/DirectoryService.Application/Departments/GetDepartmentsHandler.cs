using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Application.Departments;

public record GetDepartmentsQuery(GetDepartmentsRequest Request) : IQuery;

public class GetDepartmentsHandler : IQueryHandler<DepartmentsResponse, GetDepartmentsQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<DepartmentsResponse, Error>> Handle(
        GetDepartmentsQuery departmentsQuery,
        CancellationToken cancellationToken)
    {
        var request = departmentsQuery.Request;


        if (request.Page is < 1)
        {
            return Error.Validation(
                SharedErrorCodes.Validation.InvalidRequest,
                "Номер страницы не может быть меньше 1",
                "page");
        }

        switch (request.PageSize)
        {
            case <= 0:
                return Error.Validation(
                    SharedErrorCodes.Validation.InvalidRequest,
                    "Размер страницы не может быть меньше 0",
                    "pageSize");
            case > 100:
                return Error.Validation(
                    SharedErrorCodes.Validation.InvalidRequest,
                    "Размер страницы не может превышать 100 элементов",
                    "pageSize");
        }

        string[] allowedSortFields = ["name", "path", "date"];
        string? sortBy = request.SortBy?.Trim().ToLower();

        if (!string.IsNullOrEmpty(sortBy) && !allowedSortFields.Contains(sortBy))
        {
            return Error.Validation(
                SharedErrorCodes.Validation.InvalidRequest,
                $"Не валидное поле сортировки. Доступные поля: {string.Join(", ", allowedSortFields)}",
                "sortBy");
        }

        string? sortOrder = request.SortOrder?.Trim().ToLower();
        if (!string.IsNullOrEmpty(sortOrder) && sortOrder != "asc" && sortOrder != "desc")
        {
            return Error.Validation(
                SharedErrorCodes.Validation.InvalidRequest,
                $"Не валидное направление сортировки.",
                "sortOrder");
        }

        // Значения по умолчанию, если параметры не переданы
        int page = request.Page ?? 1;
        int pageSize = request.PageSize ?? 10;
        sortOrder ??= "asc";
        sortBy ??= "name";

        IQueryable<Department> query = _readDbContext.DepartmentsRead;

        string? cleanedSearch = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(cleanedSearch) && cleanedSearch.Length >= 3)
        {
            string searchLower = cleanedSearch.ToLower();
            query = query.Where(d => d.Name.ToLower().Contains(searchLower));
        }

        if (departmentsQuery.Request.IsActive.HasValue)
        {
            query = query.Where(l => l.IsActive == departmentsQuery.Request.IsActive);
        }

        if (departmentsQuery.Request.ExcludeIds != null && departmentsQuery.Request.ExcludeIds.Length != 0)
        {
            var excludeIds = departmentsQuery.Request.ExcludeIds.Select(i => new DepartmentId(i)).ToList();
            query = query.Where(d => !excludeIds.Contains(d.Id));
        }

        if (departmentsQuery.Request.ParentId.HasValue)
        {
            var parentId = new DepartmentId(departmentsQuery.Request.ParentId.Value);
            query = query.Where(d => d.ParentId == parentId);
        }

        if (departmentsQuery.Request.LocationIds != null && departmentsQuery.Request.LocationIds.Length != 0)
        {
            var locationIds = departmentsQuery.Request.LocationIds.Select(l => new LocationId(l)).ToList();
            query = query.Where(d => d.Locations.Any(dl => locationIds.Contains(dl.LocationId)));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        bool isAsc = sortOrder == "asc";

        query = sortBy switch
        {
            "date" => isAsc ? query.OrderBy(d => d.CreatedAt) : query.OrderByDescending(d => d.CreatedAt),
            "path" => isAsc ? query.OrderBy(d => d.Path) : query.OrderByDescending(d => d.Path),
            "name" or _ => isAsc ? query.OrderBy(d => d.Name) : query.OrderByDescending(d => d.Name)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DepartmentDto
            {
                Id = d.Id.Value,
                Name = d.Name,
                ParentId = d.ParentId != null ? d.ParentId.Value : null,
                Identifier = d.Identifier.Value,
                Path = d.Path.Value,
                Depth = d.Depth,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                DeletedAt = d.DeletedAt,
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return new DepartmentsResponse(items, totalCount);
    }
}