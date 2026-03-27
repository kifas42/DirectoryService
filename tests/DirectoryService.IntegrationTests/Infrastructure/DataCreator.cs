using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure;

namespace DirectoryService.IntegrationTests.Infrastructure;

public static class DataCreator
{
    public static async Task<LocationId> CreateLocation(ApplicationDbContext dbContext)
    {
        return await CreateLocation(dbContext, "ЛОХация", "C-1");
    }


    public static async Task<LocationId> CreateLocation(
        ApplicationDbContext dbContext,
        string name,
        string officeNumber)
    {
        var location = Location.Create(
            name,
            Address.Create(
                officeNumber,
                "b222",
                "street",
                "NightCity",
                "Ohio",
                "country",
                "11223").Value,
            Timezone.Create("Europe/London").Value);

        dbContext.Locations.Add(location.Value);
        await dbContext.SaveChangesAsync();

        return location.Value.Id;
    }

    public static async Task<Department> CreateDepartment(ApplicationDbContext dbContext, LocationId locationId)
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

        return department.Value;
    }

    public static Task<Department> CreateDepartmentNoSave(
        ApplicationDbContext dbContext,
        LocationId locationId,
        string name,
        string identifier,
        DepartmentId? departmentId,
        Department? parent)
    {
        try
        {
            departmentId ??= DepartmentId.New();
            short depth = 0;

            if (parent != null)
            {
                depth = (short)(parent.Depth + 1);
            }

            var departmentLocation = new DepartmentLocation(Guid.NewGuid(), departmentId, locationId);

            var department = Department.Create(
                departmentId,
                name,
                Identifier.Create(identifier).Value,
                parent,
                depth,
                [],
                [departmentLocation]);

            dbContext.Departments.Add(department.Value);

            return Task.FromResult(department.Value);
        }
        catch (Exception exception)
        {
            return Task.FromException<Department>(exception);
        }
    }

    public static async Task<Department> CreateDepartment(
        ApplicationDbContext dbContext,
        LocationId locationId,
        string name,
        string identifier,
        Department? parent,
        DepartmentId? departmentId = null)
    {
        var res = await CreateDepartmentNoSave(dbContext, locationId, name, identifier, departmentId, parent);

        await dbContext.SaveChangesAsync();
        return res;
    }


    public static async Task GenerateDepartmentStruct(
        ApplicationDbContext dbContext,
        TestDepartmentDto[] dtos,
        LocationId locationId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var dto in dtos)
            {
                await ProcessDepartmentNodeAsync(dbContext, locationId, dto, null);
            }

            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<Department> ProcessDepartmentNodeAsync(
        ApplicationDbContext dbContext,
        LocationId locationId,
        TestDepartmentDto dto,
        Department? parent)
    {
        var departmentId = new DepartmentId(dto.Id);

        var department = await CreateDepartmentNoSave(
            dbContext: dbContext,
            locationId: locationId,
            name: dto.Name,
            identifier: dto.Identifier,
            departmentId: departmentId,
            parent: parent
        );

        if (dto.Children.Length <= 0) return department;

        foreach (var childDto in dto.Children)
        {
            await ProcessDepartmentNodeAsync(dbContext, locationId, childDto, department);
        }

        return department;
    }

    public static TestDepartmentDto[] GetDepartmentStruct(Guid engId, Guid salesId, Guid hrId, Guid itId)
    {
        return
        [
            new TestDepartmentDto(
                engId,
                "Engineering",
                "engineering",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Eng Software Dev",
                        "eng-software-dev",
                        [
                            new TestDepartmentDto(
                                Guid.NewGuid(),
                                "Eng Soft Backend",
                                "eng-soft-backend",
                                [
                                    new TestDepartmentDto(
                                        Guid.NewGuid(),
                                        "Eng Back Core",
                                        "eng-back-core",
                                        []),
                                    new TestDepartmentDto(
                                        Guid.NewGuid(),
                                        "Eng Back Api",
                                        "eng-back-api",
                                        [])
                                ]),
                            new TestDepartmentDto(
                                Guid.NewGuid(),
                                "Eng Soft Frontend",
                                "eng-soft-frontend",
                                [])
                        ]),
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Eng Hardware Dev",
                        "eng-hardware-dev",
                        [])
                ]),
            new TestDepartmentDto(
                salesId,
                "Sales Division",
                "sales-division",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Sales Region East",
                        "sales-region-east",
                        []),
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Sales Region West",
                        "sales-region-west",
                        []),
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Sales Region North",
                        "sales-region-north",
                        []),
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Support",
                        "sales-support",
                        []),
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Sales B2B",
                        "sales-bb",
                        []),
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "ssssssssss",
                        "sssssssssss",
                        [])
                ]),
            new TestDepartmentDto(
                Guid.NewGuid(),
                "Marketing",
                "marketing",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Mkt Digital",
                        "mkt-digital",
                        [
                            new TestDepartmentDto(
                                Guid.NewGuid(),
                                "Mkt Dig SEO",
                                "mkt-dig-seo",
                                [
                                    new TestDepartmentDto(
                                        Guid.NewGuid(),
                                        "Mkt SEO Content",
                                        "mkt-seo-content",
                                        [])
                                ])
                        ])
                ]),
            new TestDepartmentDto(
                hrId,
                "HR Division",
                "hr-division",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "HR Recruiting",
                        "hr-recruiting",
                        [])
                ]),
            new TestDepartmentDto(
                Guid.NewGuid(),
                "Finance Corp",
                "finance-corp",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Fin Accounting",
                        "fin-accounting",
                        [])
                ]),
            new TestDepartmentDto(
                Guid.NewGuid(),
                "Operations",
                "operations",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Ops Supply",
                        "ops-supply",
                        [])
                ]),
            new TestDepartmentDto(
                Guid.NewGuid(),
                "Legal Team",
                "legal-team",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Leg Compliance",
                        "leg-compliance",
                        [])
                ]),
            new TestDepartmentDto(
                itId,
                "IT Support",
                "it-support",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "IT Helpdesk",
                        "it-helpdesk",
                        [
                            new TestDepartmentDto(
                                Guid.NewGuid(),
                                "IT Help Lev",
                                "it-help-lev",
                                [
                                    new TestDepartmentDto(
                                        Guid.NewGuid(),
                                        "IT Help Shift A",
                                        "it-help-shift-a",
                                        []),
                                    new TestDepartmentDto(
                                        Guid.NewGuid(),
                                        "IT Help Shift B",
                                        "it-help-shift-b",
                                        []),
                                    new TestDepartmentDto(
                                        Guid.NewGuid(),
                                        "IT Help Shift C",
                                        "it-help-shift-c",
                                        [])
                                ])
                        ])
                ]),
            new TestDepartmentDto(
                Guid.NewGuid(),
                "Research",
                "research",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Res Lab",
                        "res-lab",
                        [])
                ]),
            new TestDepartmentDto(
                Guid.NewGuid(),
                "Logistics",
                "logistics",
                [
                    new TestDepartmentDto(
                        Guid.NewGuid(),
                        "Log Transport",
                        "log-transport",
                        [])
                ])
        ];
    }
}

public record TestDepartmentDto(Guid Id, string Name, string Identifier, TestDepartmentDto[] Children);