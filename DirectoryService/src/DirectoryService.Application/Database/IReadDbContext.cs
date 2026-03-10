using System.Data.Common;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    DbConnection Connection { get; }

    IQueryable<Location> LocationsRead { get; }

    IQueryable<DepartmentLocation> DepartmentLocationsRead { get; }
}