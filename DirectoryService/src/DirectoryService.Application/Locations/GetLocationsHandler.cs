using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Application.Locations;

public record GetLocationQuery(GetLocationRequest Request) : IQuery;

public class GetLocationsHandler : IQueryHandler<PaginationLocationResponse, GetLocationQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetLocationsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<PaginationLocationResponse, Errors>> Handle(
        GetLocationQuery locationQuery,
        CancellationToken cancellationToken)
    {
        var query = _readDbContext.LocationsRead;

        if (!string.IsNullOrWhiteSpace(locationQuery.Request.Search))
        {
            query = query.Where(l => EF.Functions.Like(l.Name, $"%{locationQuery.Request.Search}%"));
        }

        if (locationQuery.Request.DepartmentIds is not null && locationQuery.Request.DepartmentIds.Length != 0)
        {
            var departmentIds = locationQuery.Request.DepartmentIds
                .Select(id => new DepartmentId(id))
                .ToList();

            query = (from l in query
                join dl in _readDbContext.DepartmentLocationsRead on l.Id equals dl.LocationId
                where departmentIds.Contains(dl.DepartmentId)
                select l).Distinct();
        }

        if (locationQuery.Request.IsActive is not null)
        {
            query = query.Where(l => l.IsActive == locationQuery.Request.IsActive);
        }

        Expression<Func<Location, object>> keySelector = locationQuery.Request.SortBy.ToLower() switch
        {
            "name" => l => l.Name,
            "date" => l => l.CreatedAt,
            _ => l => l.Name
        };

        query = locationQuery.Request.SortOrder == "asc"
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);

        int locationsCount = await query.CountAsync(cancellationToken);

        var locations = await query
            .Select(l => new GetLocationDto()
            {
                Name = l.Name,
                OfficeNumber = l.Address.OfficeNumber,
                BuildingNumber = l.Address.BuildingNumber,
                Street = l.Address.Street,
                City = l.Address.City,
                StateOrProvince = l.Address.StateOrProvince,
                Country = l.Address.Country,
                PostalCode = l.Address.PostalCode,
                Timezone = l.Timezone.Value,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
            })
            .Skip((locationQuery.Request.Page - 1) * locationQuery.Request.PageSize ?? 0)
            .Take(locationQuery.Request.PageSize ?? 10)
            .ToListAsync(cancellationToken);


        return new PaginationLocationResponse(locations, locationsCount);
    }
}