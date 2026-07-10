using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Shared;

namespace DirectoryService.IntegrationTests.Infrastructure;

public static class DataCreator
{
    public static async Task<LocationId> CreateLocation(ApplicationDbContext dbContext) =>
        await CreateLocation(dbContext, "ЛОХация", "C-1");

    public static async Task<LocationId> CreateLocation(
        ApplicationDbContext dbContext,
        string name,
        string officeNumber)
    {
        Result<Location, Error> location = Location.Create(
            LocationId.New(),
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
        DepartmentId departmentId = DepartmentId.New();
        DepartmentLocation departmentLocation = new(Guid.NewGuid(), departmentId, locationId);

        Result<Department, Error> department = Department.Create(
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
        IEnumerable<LocationId> locationIds,
        IEnumerable<PositionId> positionIds,
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

            List<DepartmentPosition> departmentPositions =
                positionIds.Select(x =>
                        new DepartmentPosition(Guid.NewGuid(), departmentId, new PositionId(x.Value)))
                    .ToList();

            List<DepartmentLocation> departmentLocations =
                locationIds.Select(x =>
                        new DepartmentLocation(Guid.NewGuid(), departmentId, new LocationId(x.Value)))
                    .ToList();

            Result<Department, Error> department = Department.Create(
                departmentId,
                name,
                Identifier.Create(identifier).Value,
                parent,
                depth,
                departmentPositions,
                departmentLocations);

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
        IEnumerable<LocationId> locationIds,
        IEnumerable<PositionId> positionIds,
        string name,
        string identifier,
        Department? parent,
        DepartmentId? departmentId = null)
    {
        Department res = await CreateDepartmentNoSave(dbContext, locationIds, positionIds, name, identifier,
            departmentId, parent);

        await dbContext.SaveChangesAsync();
        return res;
    }

    public static async Task GenerateDepartmentStruct(
        ApplicationDbContext dbContext,
        TestDepartmentDto[] dtos,
        IReadOnlyList<LocationId> locationIds,
        IReadOnlyList<PositionId> positionIds)
    {
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (TestDepartmentDto dto in dtos)
            {
                await ProcessDepartmentNodeAsync(dbContext, locationIds, positionIds, dto, null);
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

    public static TestDepartmentDto[] GetDepartmentStruct(Guid engId, Guid salesId, Guid hrId, Guid itId) =>
    [
        new(
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
        new(
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
        new(
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
        new(
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
        new(
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
        new(
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
        new(
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
        new(
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
        new(
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
        new(
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

    public static async Task<IReadOnlyList<PositionId>> CreatePositions(
        ApplicationDbContext dbContext,
        IReadOnlyList<TestPositionDto> testPositions,
        CancellationToken cancellationToken = default)
    {
        List<Position> positions = testPositions.Select(x => Position.Create(
            new PositionId(x.Id),
            x.Name,
            null,
            []))
            .Select(y => y.Value).ToList();

        await dbContext.Positions.AddRangeAsync(positions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return positions.Select(x => x.Id).ToList();
    }

    private static async Task ProcessDepartmentNodeAsync(
        ApplicationDbContext dbContext,
        IReadOnlyList<LocationId> locationIds,
        IReadOnlyList<PositionId> positionIds,
        TestDepartmentDto dto,
        Department? parent)
    {
        DepartmentId departmentId = new(dto.Id);

        Department department = await CreateDepartmentNoSave(
            dbContext,
            locationIds,
            positionIds,
            dto.Name,
            dto.Identifier,
            departmentId,
            parent);

        if (dto.Children.Length <= 0)
        {
            return;
        }

        foreach (TestDepartmentDto childDto in dto.Children)
        {
            await ProcessDepartmentNodeAsync(dbContext, locationIds, positionIds, childDto, department);
        }
    }
}

public record TestDepartmentDto(Guid Id, string Name, string Identifier, TestDepartmentDto[] Children);

public record TestPositionDto(Guid Id, string Name);