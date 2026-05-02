using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class GetRootDepartmentsTest(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    private readonly Guid _engId = Guid.NewGuid();
    private readonly Guid _hrId = Guid.NewGuid();
    private readonly Guid _itId = Guid.NewGuid();
    private readonly Guid _salesId = Guid.NewGuid();

    [Fact]
    public async Task GetRoots_WithValidData_ShouldReturnChildren()
    {
        LocationId? locationId;
        CancellationToken cancellationToken = CancellationToken.None;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);

            await DataCreator.GenerateDepartmentStruct(
                dbContext,
                DataCreator.GetDepartmentStruct(_engId, _salesId, _hrId, _itId),
                [locationId],
                []);
        });

        Result<DepartmentsResponse, Error> result =
            await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>(sut =>
            {
                GetRootDepartmentsQuery command = new(
                    new RootDepartmentsRequest(
                        2,
                        1)
                );

                return sut.Handle(command, cancellationToken);
            });

        Result<DepartmentsResponse, Error> result2 =
            await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>(sut =>
            {
                GetRootDepartmentsQuery command = new(
                    new RootDepartmentsRequest(
                        2,
                        1,
                        30)
                );

                return sut.Handle(command, cancellationToken);
            });

        Result<DepartmentsResponse, Error> result3 =
            await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>(sut =>
            {
                GetRootDepartmentsQuery command = new(
                    new RootDepartmentsRequest(
                        1,
                        20,
                        4)
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