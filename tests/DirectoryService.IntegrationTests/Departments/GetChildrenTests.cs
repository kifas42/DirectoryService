using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

public class GetChildrenTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    private readonly Guid _engId = Guid.NewGuid();
    private readonly Guid _salesId = Guid.NewGuid();
    private readonly Guid _hrId = Guid.NewGuid();
    private readonly Guid _itId = Guid.NewGuid();


    [Fact]
    public async Task GetChildren_WithValidData_ShouldReturnChildren()
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


        var result = await ExecuteHandler<DepartmentsResponse, GetChildrenHandler>((sut) =>
        {
            var command = new GetChildDepartmentsQuery(
                _salesId,
                new ChildDepartmentsRequest()
            );

            return sut.Handle(command, cancellationToken);
        });

        var result2 = await ExecuteHandler<DepartmentsResponse, GetChildrenHandler>((sut) =>
        {
            var command = new GetChildDepartmentsQuery(
                _salesId,
                new ChildDepartmentsRequest(
                    Page: 2,
                    Size: 2)
            );

            return sut.Handle(command, cancellationToken);
        });

        var result3 = await ExecuteHandler<DepartmentsResponse, GetChildrenHandler>((sut) =>
        {
            var command = new GetChildDepartmentsQuery(
                _itId,
                new ChildDepartmentsRequest()
            );

            return sut.Handle(command, cancellationToken);
        });

        Assert.NotNull(result.Value);
        Assert.Equal(6, result.Value.Departments.Count);

        Assert.NotNull(result2.Value);
        Assert.Equal(2, result2.Value.Departments.Count);
        Assert.Equal("sales-region-north", result2.Value.Departments[0].Identifier);

        Assert.NotNull(result3.Value);
        Assert.Single(result3.Value.Departments);
        Assert.Equal("it-helpdesk", result3.Value.Departments[0].Identifier);
        Assert.True(result3.Value.Departments[0].HasMoreChildren);
    }
}