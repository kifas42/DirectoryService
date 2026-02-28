using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateParentTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    /*

    root_dep_A
       |---dep_A1
           |---dep_A2

    root_dep_B
       |---dep_B1
           |---dep_B2

   move:

    root_dep_A
        |---dep_A1
            |---dep_A2
            |---dep_B1
                |---dep_B2

    root_dep_B
       |

*/

    [Fact]
    public async Task UpdateParents_WithValidData_ShouldSucceed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId;
        Department rootDepartmentA;
        Department rootDepartmentB;
        Department departmentA1 = null!;
        Department departmentA2 = null!;
        Department departmentB1 = null!;
        Department departmentB2 = null!;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);

            rootDepartmentA = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentA",
                "dep-a",
                null);

            rootDepartmentB = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB",
                "dep-b",
                null);

            departmentA1 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentA1",
                "dep-aa",
                rootDepartmentA);

            departmentA2 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentA2",
                "dep-aaa",
                departmentA1);

            departmentB1 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB1",
                "dep-bb",
                rootDepartmentB);

            departmentB2 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB2",
                "dep-bbb",
                departmentB1);
        });

        // act

        var result = await ExecuteHandler<int, UpdateParentHandler>((sut) =>
        {
            // переносим B1 под A1
            var command = new UpdateParentCommand(
                departmentB1.Id.Value,
                new UpdateParentRequest()
                {
                    ParentId = departmentA1.Id.Value,
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert

        // must be
        // dep-a
        // dep-a.dep-aa
        // dep-a.dep-aa.dep-aaa
        // dep-a.dep-aa.dep-bb
        // dep-a.dep-aa.dep-bb.dep-bbb
        // dep-b
        await ExecuteInDb(async dbContext =>
        {
            var depAaa = await dbContext.Departments
                .Where(d => d.Id == departmentA2.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            var depBb = await dbContext.Departments
                .Where(d => d.Id == departmentB1.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            var depBbb = await dbContext.Departments
                .Where(d => d.Id == departmentB2.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);
            
            Assert.True(result.IsSuccess);

            // dep-aaa
            Assert.NotNull(depAaa);
            Assert.NotNull(depAaa.Parent);
            Assert.Equal(2, depAaa.Depth);
            Assert.Equal("dep-a.dep-aa.dep-aaa", depAaa.Path.Value);
            Assert.Equal(departmentA1.Id, depAaa.Parent.Id);

            // dep-bb
            Assert.NotNull(depBb);
            Assert.NotNull(depBb.Parent);
            Assert.Equal(2, depBb.Depth);
            Assert.Equal("dep-a.dep-aa.dep-bb", depBb.Path.Value);
            Assert.Equal(departmentA1.Id, depBb.Parent.Id);

            // dep-bbb
            Assert.NotNull(depBbb);
            Assert.NotNull(depBbb.Parent);
            Assert.Equal(3, depBbb.Depth);
            Assert.Equal("dep-a.dep-aa.dep-bb.dep-bbb", depBbb.Path.Value);
            Assert.Equal(departmentB1.Id, depBbb.Parent.Id);
        });
    }

    [Fact]
    public async Task RemoveParent_WithValidData_ShouldSucceed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId;
        Department rootDepartmentA;
        Department departmentA1 = null!;
        Department departmentA2 = null!;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);

            rootDepartmentA = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentA",
                "dep-a",
                null);

            departmentA1 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentA1",
                "dep-aa",
                rootDepartmentA);

            departmentA2 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentA2",
                "dep-aaa",
                departmentA1);
        });

        // act

        var result = await ExecuteHandler<int, UpdateParentHandler>((sut) =>
        {
            // переносим A1 как корень
            var command = new UpdateParentCommand(
                departmentA1.Id.Value,
                new UpdateParentRequest()
                {
                    ParentId = null,
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert

        // must be
        // dep-a
        // dep-aa
        // dep-aa.dep-aaa
        await ExecuteInDb(async dbContext =>
        {
            var depAaa = await dbContext.Departments
                .Where(d => d.Id == departmentA2.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            var depAa = await dbContext.Departments
                .Where(d => d.Id == departmentA1.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            Assert.True(result.IsSuccess);

            // dep-aaa
            Assert.NotNull(depAaa);
            Assert.NotNull(depAaa.Parent);
            Assert.Equal(1, depAaa.Depth);
            Assert.Equal("dep-aa.dep-aaa", depAaa.Path.Value);
            Assert.Equal(departmentA1.Id, depAaa.Parent.Id);

            // dep-aa
            Assert.NotNull(depAa);
            Assert.Null(depAa.Parent);
            Assert.Equal(0, depAa.Depth);
            Assert.Equal("dep-aa", depAa.Path.Value);
        });
    }

    [Fact]
    public async Task UpdateParents_WithCyclicChild_ShouldFailed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId;
        Department rootDepartmentB = null!;
        Department departmentB1 = null!;
        Department departmentB2 = null!;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);

            rootDepartmentB = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB",
                "dep-b",
                null);

            departmentB1 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB1",
                "dep-bb",
                rootDepartmentB);

            departmentB2 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB2",
                "dep-bbb",
                departmentB1);
        });

        // act

        var result = await ExecuteHandler<int, UpdateParentHandler>((sut) =>
        {
            // переносим root B под B2
            var command = new UpdateParentCommand(
                rootDepartmentB.Id.Value,
                new UpdateParentRequest()
                {
                    ParentId = departmentB2.Id.Value,
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert

        // must be
        // dep-b
        // dep-b.dep-bb
        // dep-b.dep-bb.dep-bbb
        await ExecuteInDb(async dbContext =>
        {
            var depB = await dbContext.Departments
                .Where(d => d.Id == rootDepartmentB.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            var depBb = await dbContext.Departments
                .Where(d => d.Id == departmentB1.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            var depBbb = await dbContext.Departments
                .Where(d => d.Id == departmentB2.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            Assert.True(result.IsFailure);

            // dep-b
            Assert.NotNull(depB);
            Assert.Null(depB.Parent);
            Assert.Equal(0, depB.Depth);
            Assert.Equal("dep-b", depB.Path.Value);

            // dep-bb
            Assert.NotNull(depBb);
            Assert.NotNull(depBb.Parent);
            Assert.Equal(1, depBb.Depth);
            Assert.Equal("dep-b.dep-bb", depBb.Path.Value);
            Assert.Equal(rootDepartmentB.Id, depBb.Parent.Id);

            // dep-bbb
            Assert.NotNull(depBbb);
            Assert.NotNull(depBbb.Parent);
            Assert.Equal(2, depBbb.Depth);
            Assert.Equal("dep-b.dep-bb.dep-bbb", depBbb.Path.Value);
            Assert.Equal(departmentB1.Id, depBbb.Parent.Id);
        });
    }

    [Fact]
    public async Task UpdateParents_WithNonExistParent_ShouldFailed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId;
        Department rootDepartmentB = null!;
        Department departmentB1 = null!;
        Department departmentB2 = null!;

        var fakeParentId = DepartmentId.New();

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);

            rootDepartmentB = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB",
                "dep-b",
                null);

            departmentB1 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB1",
                "dep-bb",
                rootDepartmentB);

            departmentB2 = await DataCreator.CreateDepartment(
                dbContext,
                locationId,
                "departmentB2",
                "dep-bbb",
                departmentB1);
        });

        // act

        var result = await ExecuteHandler<int, UpdateParentHandler>((sut) =>
        {
            // переносим root B под fake parent
            var command = new UpdateParentCommand(
                rootDepartmentB.Id.Value,
                new UpdateParentRequest()
                {
                    ParentId = fakeParentId.Value,
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert

        // must be
        // dep-b
        // dep-b.dep-bb
        // dep-b.dep-bb.dep-bbb
        await ExecuteInDb(async dbContext =>
        {
            var depB = await dbContext.Departments
                .Where(d => d.Id == rootDepartmentB.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            var depBb = await dbContext.Departments
                .Where(d => d.Id == departmentB1.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            var depBbb = await dbContext.Departments
                .Where(d => d.Id == departmentB2.Id)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(cancellationToken);

            Assert.True(result.IsFailure);

            // dep-b
            Assert.NotNull(depB);
            Assert.Null(depB.Parent);
            Assert.Equal(0, depB.Depth);
            Assert.Equal("dep-b", depB.Path.Value);

            // dep-bb
            Assert.NotNull(depBb);
            Assert.NotNull(depBb.Parent);
            Assert.Equal(1, depBb.Depth);
            Assert.Equal("dep-b.dep-bb", depBb.Path.Value);
            Assert.Equal(rootDepartmentB.Id, depBb.Parent.Id);

            // dep-bbb
            Assert.NotNull(depBbb);
            Assert.NotNull(depBbb.Parent);
            Assert.Equal(2, depBbb.Depth);
            Assert.Equal("dep-b.dep-bb.dep-bbb", depBbb.Path.Value);
            Assert.Equal(departmentB1.Id, depBbb.Parent.Id);
        });
    }
}