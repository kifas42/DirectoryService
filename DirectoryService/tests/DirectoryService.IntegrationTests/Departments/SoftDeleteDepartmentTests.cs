using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.IntegrationTests.Departments;

//        loc1   loc2   loc3   loc4
// dep1    X      X
// dep2           X
// dep3                  X
// dep4                          X

//        pos1   pos2   pos3   pos4
// dep1    X      X
// dep2           X
// dep3                  X
// dep4                          X

public class SoftDeleteDepartmentTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task DeleteDepartment_ShouldSucceed()
    {
        var cancellationToken = CancellationToken.None;
        LocationId location1Id = null!;
        LocationId location2Id = null!;
        LocationId location3Id = null!;
        LocationId location4Id = null!;

        IReadOnlyList<TestPositionDto> testPositionDto =
        [
            new(Guid.NewGuid(), "pos1"),
            new(Guid.NewGuid(), "pos2"),
            new(Guid.NewGuid(), "pos3"),
            new(Guid.NewGuid(), "pos4")
        ];

        PositionId position1Id = null!;
        PositionId position2Id = null!;
        PositionId position3Id = null!;
        PositionId position4Id = null!;

        DepartmentId department1Id = null!;
        DepartmentId department2Id = null!;
        DepartmentId department3Id = null!;
        DepartmentId department4Id = null!;

        await ExecuteInDb(async dbContext =>
        {
            location1Id = await DataCreator.CreateLocation(dbContext, "Location-1", "B1");
            location2Id = await DataCreator.CreateLocation(dbContext, "Location-2", "B3");
            location3Id = await DataCreator.CreateLocation(dbContext, "Location-3", "B3");
            location4Id = await DataCreator.CreateLocation(dbContext, "Location-4", "B4");

            var arr = await DataCreator.CreatePositions(dbContext, testPositionDto, cancellationToken);
            position1Id = arr[0];
            position2Id = arr[1];
            position3Id = arr[2];
            position4Id = arr[3];

            var dep1 = await DataCreator.CreateDepartment(
                dbContext,
                [location1Id, location2Id],
                [position1Id, position2Id],
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
                null);
            department2Id = dep2.Id;

            var dep3 = await DataCreator.CreateDepartment(
                dbContext,
                [location3Id],
                [position3Id],
                "CCC",
                "ccc",
                dep1);
            department3Id = dep3.Id;

            var dep4 = await DataCreator.CreateDepartment(
                dbContext,
                [location4Id],
                [position4Id],
                "DDD",
                "ddd",
                dep3);
            department4Id = dep4.Id;
        });


        var result = await ExecuteHandler<SoftDeleteDepartmentHandler>((sut) =>
        {
            var command = new DeleteDepartmentCommand(department1Id.Value);

            return sut.Handle(command, cancellationToken);
        });

        var result2 = await ExecuteHandler<SoftDeleteDepartmentHandler>((sut) =>
        {
            var command = new DeleteDepartmentCommand(department4Id.Value);

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments.Where(d => d.Id == department1Id)
                .FirstOrDefaultAsync(cancellationToken);

            var child3Department = await dbContext.Departments.Where(d => d.Id == department3Id)
                .FirstOrDefaultAsync(cancellationToken);

            var deletedChild4 = await dbContext.Departments.Where(d => d.Id == department4Id)
                .FirstOrDefaultAsync(cancellationToken);


            var anotherDepartment = await dbContext.Departments.Where(d => d.Id == department2Id)
                .FirstOrDefaultAsync(cancellationToken);


            var deletedLocation = await dbContext.Locations.Where(l => l.Id == location1Id)
                .FirstOrDefaultAsync(cancellationToken);

            var deletedLocation4 = await dbContext.Locations.Where(l => l.Id == location4Id)
                .FirstOrDefaultAsync(cancellationToken);

            var linkedLocation2 = await dbContext.Locations.Where(l => l.Id == location2Id)
                .FirstOrDefaultAsync(cancellationToken);


            var deletedPosition = await dbContext.Positions.Where(l => l.Id == position1Id)
                .FirstOrDefaultAsync(cancellationToken);

            var deletedPosition4 = await dbContext.Positions.Where(l => l.Id == position4Id)
                .FirstOrDefaultAsync(cancellationToken);

            var linkedPosition2 = await dbContext.Positions.Where(l => l.Id == position2Id)
                .FirstOrDefaultAsync(cancellationToken);


            Assert.True(result.IsSuccess);
            Assert.True(result2.IsSuccess);

            // Dep1 AAA
            Assert.NotNull(deletedDepartment);
            Assert.False(deletedDepartment.IsActive);
            Assert.NotNull(deletedDepartment.DeletedAt);
            Assert.Equal(deletedDepartment.UpdatedAt, deletedDepartment.DeletedAt);
            Assert.Equal("deleted-aaa", deletedDepartment.Identifier.Value);
            Assert.Equal("deleted-aaa", deletedDepartment.Path.Value);


            // Dep2 BBB
            Assert.NotNull(anotherDepartment);
            Assert.True(anotherDepartment.IsActive);

            // Dep3 CCC
            Assert.NotNull(child3Department);
            Assert.True(child3Department.IsActive);
            Assert.Null(child3Department.DeletedAt);
            Assert.Equal("deleted-aaa.ccc", child3Department.Path.Value);

            // Dep4 DDD
            Assert.NotNull(deletedChild4);
            Assert.False(deletedChild4.IsActive);
            Assert.NotNull(deletedChild4.DeletedAt);
            Assert.Equal("deleted-ddd", deletedChild4.Identifier.Value);
            Assert.Equal("deleted-aaa.ccc.deleted-ddd", deletedChild4.Path.Value);

            // assert locations
            Assert.NotNull(deletedLocation);
            Assert.False(deletedLocation.IsActive);

            Assert.NotNull(deletedLocation4);
            Assert.False(deletedLocation4.IsActive);

            Assert.NotNull(linkedLocation2);
            Assert.True(linkedLocation2.IsActive);

            // assert positions
            Assert.NotNull(deletedPosition);
            Assert.False(deletedPosition.IsActive);

            Assert.NotNull(deletedPosition4);
            Assert.False(deletedPosition4.IsActive);

            Assert.NotNull(linkedPosition2);
            Assert.True(linkedPosition2.IsActive);
        });
    }

    [Fact]
    public async Task DeleteDepartment_WithNotActive_ShouldReturnConflict()
    {
        var cancellationToken = CancellationToken.None;
        LocationId location1Id = null!;

        IReadOnlyList<TestPositionDto> testPositionDto =
        [
            new(Guid.NewGuid(), "pos1"),
        ];

        PositionId position1Id = null!;

        DepartmentId department1Id = null!;

        await ExecuteInDb(async dbContext =>
        {
            location1Id = await DataCreator.CreateLocation(dbContext, "Location-1", "B1");

            var arr = await DataCreator.CreatePositions(dbContext, testPositionDto, cancellationToken);
            position1Id = arr[0];

            var dep1 = await DataCreator.CreateDepartment(
                dbContext,
                [location1Id],
                [position1Id],
                "AAA",
                "aaa",
                null);
            department1Id = dep1.Id;
        });


        var result = await ExecuteHandler<SoftDeleteDepartmentHandler>((sut) =>
        {
            var command = new DeleteDepartmentCommand(department1Id.Value);

            return sut.Handle(command, cancellationToken);
        });

        var result2 = await ExecuteHandler<SoftDeleteDepartmentHandler>((sut) =>
        {
            var command = new DeleteDepartmentCommand(department1Id.Value);

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments.Where(d => d.Id == department1Id)
                .FirstOrDefaultAsync(cancellationToken);

            var deletedLocation = await dbContext.Locations.Where(l => l.Id == location1Id)
                .FirstOrDefaultAsync(cancellationToken);

            var deletedPosition = await dbContext.Positions.Where(l => l.Id == position1Id)
                .FirstOrDefaultAsync(cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.True(result2.IsFailure);

            Assert.Equal(ErrorType.CONFLICT, result2.Error.Type);

            // Dep1 AAA
            Assert.NotNull(deletedDepartment);
            Assert.False(deletedDepartment.IsActive);
            Assert.Equal("deleted-aaa", deletedDepartment.Path.Value);

            // assert locations
            Assert.NotNull(deletedLocation);
            Assert.False(deletedLocation.IsActive);

            // assert positions
            Assert.NotNull(deletedPosition);
            Assert.False(deletedPosition.IsActive);
        });
    }

    [Fact]
    public async Task DeleteDepartment_OnNonExistDepartment_ShouldReturnNotFound()
    {
        var cancellationToken = CancellationToken.None;
        LocationId location1Id = null!;

        IReadOnlyList<TestPositionDto> testPositionDto =
        [
            new(Guid.NewGuid(), "pos1"),
        ];

        PositionId position1Id = null!;

        DepartmentId department1Id = null!;

        var noneExistsDepartmentId = DepartmentId.New();

        await ExecuteInDb(async dbContext =>
        {
            location1Id = await DataCreator.CreateLocation(dbContext, "Location-1", "B1");

            var arr = await DataCreator.CreatePositions(dbContext, testPositionDto, cancellationToken);
            position1Id = arr[0];

            var dep1 = await DataCreator.CreateDepartment(
                dbContext,
                [location1Id],
                [position1Id],
                "AAA",
                "aaa",
                null);
            department1Id = dep1.Id;
        });


        var result = await ExecuteHandler<SoftDeleteDepartmentHandler>((sut) =>
        {
            var command = new DeleteDepartmentCommand(noneExistsDepartmentId.Value);

            return sut.Handle(command, cancellationToken);
        });


        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments.Where(d => d.Id == department1Id)
                .FirstOrDefaultAsync(cancellationToken);

            var deletedLocation = await dbContext.Locations.Where(l => l.Id == location1Id)
                .FirstOrDefaultAsync(cancellationToken);

            var deletedPosition = await dbContext.Positions.Where(l => l.Id == position1Id)
                .FirstOrDefaultAsync(cancellationToken);

            Assert.True(result.IsFailure);

            Assert.Equal(ErrorType.NOT_FOUND, result.Error.Type);

            // Dep1 AAA
            Assert.NotNull(deletedDepartment);
            Assert.True(deletedDepartment.IsActive);

            // assert locations
            Assert.NotNull(deletedLocation);
            Assert.True(deletedLocation.IsActive);

            // assert positions
            Assert.NotNull(deletedPosition);
            Assert.True(deletedPosition.IsActive);
        });
    }
}