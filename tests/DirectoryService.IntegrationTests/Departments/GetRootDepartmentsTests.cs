using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

public class GetRootDepartmentsTest(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    private readonly Guid _engId = Guid.NewGuid();
    private readonly Guid _salesId = Guid.NewGuid();
    private readonly Guid _hrId = Guid.NewGuid();
    private readonly Guid _itId = Guid.NewGuid();


    [Fact]
    public async Task GetRoots_WithValidData_ShouldReturnChildren()
    {
        LocationId? locationId;
        var cancellationToken = CancellationToken.None;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);

            await DataCreator.GenerateDepartmentStruct(
                dbContext,
                DataCreator.GetDepartmentStruct(_engId, _salesId, _hrId, _itId),
                locationId);
        });


        var result = await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>((sut) =>
        {
            var command = new GetRootDepartmentsQuery(
                new RootDepartmentsRequest(
                    Page: 2,
                    Size: 1,
                    Prefetch: 3)
            );

            return sut.Handle(command, cancellationToken);
        });

        var result2 = await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>((sut) =>
        {
            var command = new GetRootDepartmentsQuery(
                new RootDepartmentsRequest(
                    Page: 2,
                    Size: 1,
                    Prefetch: 30)
            );

            return sut.Handle(command, cancellationToken);
        });

        var result3 = await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>((sut) =>
        {
            var command = new GetRootDepartmentsQuery(
                new RootDepartmentsRequest(
                    Page: 1,
                    Size: 20,
                    Prefetch: 4)
            );

            return sut.Handle(command, cancellationToken);
        });


        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Departments);
        Assert.Equal(3, result.Value.Departments[0].Children.Count);
        Assert.True(result.Value.Departments[0].HasMoreChildren);

        Assert.NotNull(result2.Value);
        Assert.Single(result2.Value.Departments);
        Assert.Equal(6, result2.Value.Departments[0].Children.Count);
        Assert.False(result2.Value.Departments[0].HasMoreChildren);

        Assert.NotNull(result3.Value);
        Assert.Equal(10, result3.Value.Departments.Count);
        Assert.Equal(4, result3.Value.Departments.First(d => d.Id == _salesId).Children.Count);
        Assert.True(result3.Value.Departments.First(d => d.Id == _salesId).HasMoreChildren);
    }
}