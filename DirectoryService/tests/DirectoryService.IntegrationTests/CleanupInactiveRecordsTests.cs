using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class CleanupInactiveRecordsTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task CleanupInactiveRecords_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        LocationId location1Id = null!;
        LocationId location2Id = null!;
        
        IReadOnlyList<TestPositionDto> testPositionDto =
        [
            new(Guid.NewGuid(), "pos1"),
            new(Guid.NewGuid(), "pos2"),
        ];
        
        PositionId position1Id = null!;
        PositionId position2Id = null!;
        
        DepartmentId department1Id = null!;
        DepartmentId department2Id = null!;
        DepartmentId department3Id = null!;
        
        await ExecuteInDb(async dbContext =>
        {
            location1Id = await DataCreator.CreateLocation(dbContext, "Location-1", "B1");
            location2Id = await DataCreator.CreateLocation(dbContext, "Location-2", "B3");

            var arr = await DataCreator.CreatePositions(dbContext, testPositionDto, cancellationToken);
            position1Id = arr[0];
            position2Id = arr[1];

            var dep1 = await DataCreator.CreateDepartment(
                dbContext,
                [location1Id],
                [position1Id],
                "AAA",
                "aaa",
                null);
            department1Id = dep1.Id;

            var dep2 = await DataCreator.CreateDepartment(
                dbContext,
                [location2Id],
                [position2Id],
                "BBB",
                "bbb",
                dep1);
            department2Id = dep2.Id;

            var dep3 = await DataCreator.CreateDepartment(
                dbContext,
                [location2Id],
                [position2Id],
                "CCC",
                "ccc",
                dep2);
            department3Id = dep3.Id;
            
        });
        
        var softDeleteResult = await ExecuteHandler<SoftDeleteDepartmentHandler>((sut) =>
        {
            var command = new DeleteDepartmentCommand(department1Id.Value);

            return sut.Handle(command, cancellationToken);
        });
        
        
        await using var scope = Services.CreateAsyncScope();
        var cleanupService = scope.ServiceProvider.GetRequiredService<CleanupInactiveRecordsService>();
        
        await cleanupService.RunCleanupAsync(cancellationToken);

        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments.Where(d => d.Id == department1Id)
                .Include(d=>d.Parent)
                .FirstOrDefaultAsync(cancellationToken);
            
            var child2Department = await dbContext.Departments.Where(d => d.Id == department2Id)
                .Include(d=>d.Parent)
                .FirstOrDefaultAsync(cancellationToken);
            
            var child3Department = await dbContext.Departments.Where(d => d.Id == department3Id)
                .Include(d=>d.Parent)
                .FirstOrDefaultAsync(cancellationToken);
            
            var deletedLocation1 = await dbContext.Locations.Where(l => l.Id == location1Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var linkedLocation2 = await dbContext.Locations.Where(l => l.Id == location2Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var department1Location = await dbContext.DepartmentLocations.Where(dl=>dl.DepartmentId == department1Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var department2Location = await dbContext.DepartmentLocations.Where(dl=>dl.DepartmentId == department2Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var department3Location = await dbContext.DepartmentLocations.Where(dl=>dl.DepartmentId == department3Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var deletedPosition1 = await dbContext.Positions.Where(l => l.Id == position1Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var linkedPosition2 = await dbContext.Positions.Where(l => l.Id == position2Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var departmentPosition1 = await dbContext.DepartmentPositions.Where(dp=>dp.DepartmentId == department1Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var departmentPosition2 = await dbContext.DepartmentPositions.Where(dp=>dp.DepartmentId == department2Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            var departmentPosition3 = await dbContext.DepartmentPositions.Where(dp=>dp.DepartmentId == department3Id)
                .FirstOrDefaultAsync(cancellationToken);
            
            
            Assert.True(softDeleteResult.IsSuccess);
            // dep1
            Assert.Null(deletedDepartment);
            
            // dep2
            Assert.NotNull(child2Department);
            Assert.Equal("bbb", child2Department.Path.Value);
            Assert.Null(child2Department.Parent);
            
            //dep3
            Assert.NotNull(child3Department);
            Assert.Equal("bbb.ccc", child3Department.Path.Value);
            Assert.NotNull(child3Department.Parent);
            Assert.Equal(department2Id, child3Department.Parent.Id);
            
            // loc1
            Assert.Null(deletedLocation1);
            
            // loc2
            Assert.NotNull(linkedLocation2);
            
            // pos1
            Assert.Null(deletedPosition1);
            
            // pos2
            Assert.NotNull(linkedPosition2);
            
            // department_locations
            Assert.Null(department1Location);
            Assert.NotNull(department2Location);
            Assert.NotNull(department3Location);
            
            // department_positions
            Assert.Null(departmentPosition1);
            Assert.NotNull(departmentPosition2);
            Assert.NotNull(departmentPosition3);
        });
    }
}