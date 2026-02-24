using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

public static class DataCreator
{
    public static async Task<LocationId> CreateLocation(ApplicationDbContext dbContext)
    {
        return await DataCreator.CreateLocation(dbContext, "ЛОХация", "C-1");
    }


    public static async Task<LocationId> CreateLocation(ApplicationDbContext dbContext, string name,
        string officeNumber)
    {
        var location = Location.Create(
            name,
            Address.Create(
                officeNumber,
                "b222",
                "street",
                "NightCty",
                "Ohio",
                "country",
                "11223").Value,
            Timezone.Create("Europe/London").Value);

        dbContext.Locations.Add(location.Value);
        await dbContext.SaveChangesAsync();
        return location.Value.Id;
    }

    public static async Task<DepartmentId> CreateDepartment(ApplicationDbContext dbContext, LocationId locationId)
    {
        var departmentId = DepartmentId.New();
        var departmentLocation = new DepartmentLocation(Guid.NewGuid(), departmentId, locationId);

        var department = Department.Create(
            departmentId,
            "Dev Team",
            Identifier.Create("dev-team").Value,
            null,
            0,
            [],
            [departmentLocation]);

        dbContext.Departments.Add(department.Value);
        await dbContext.SaveChangesAsync();
        return department.Value.Id;
    }
}