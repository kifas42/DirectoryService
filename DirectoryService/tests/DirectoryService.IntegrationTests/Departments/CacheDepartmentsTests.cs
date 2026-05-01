using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

public class CacheDepartmentsTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    private readonly DirectoryTestWebFactory _factory = factory;

    [Fact]
    public async Task GetDepartments_WhenCacheMiss_ShouldWriteToRedis()
    {
        LocationId? locationId = null;
        await ExecuteInDb(async dbContext => { locationId = await DataCreator.CreateLocation(dbContext); });
        var cancellationToken = CancellationToken.None;
        await ExecuteHandler<Guid, CreateDepartmentHandler>((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest()
                {
                    Name = "Подразделение",
                    Identifier = "dep",
                    ParentId = null,
                    LocationIds = [locationId!.Value],
                }
            );

            return sut.Handle(command, cancellationToken);
        });
        var request = new RootDepartmentsRequest(
            Page: 1,
            Size: 2,
            Prefetch: 2);
        var rr = await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>((sut) =>
        {
            var command = new GetRootDepartmentsQuery(request);

            return sut.Handle(command, cancellationToken);
        });

        await Task.Delay(300, cancellationToken);

        var key = new RootDepartmentsCacheKey(request);
        Assert.True(await _factory.RedisDb.KeyExistsAsync(key.Value));
        Assert.True(await _factory.RedisDb.KeyTimeToLiveAsync(key.Value) > TimeSpan.Zero);
    }
    
    [Fact]
    public async Task UpdateDepartments_CacheShouldInvalidated()
    {
        LocationId? locationId = null;
        await ExecuteInDb(async dbContext => { locationId = await DataCreator.CreateLocation(dbContext); });
        var cancellationToken = CancellationToken.None;
        var createDepartmentResult = await ExecuteHandler<Guid, CreateDepartmentHandler>((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest()
                {
                    Name = "Подразделение",
                    Identifier = "dep-a",
                    ParentId = null,
                    LocationIds = [locationId!.Value],
                }
            );

            return sut.Handle(command, cancellationToken);
        });
        
        var createDepartmentResult2 = await ExecuteHandler<Guid, CreateDepartmentHandler>((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest()
                {
                    Name = "Подразделение2",
                    Identifier = "dep-b",
                    ParentId = null,
                    LocationIds = [locationId!.Value],
                }
            );

            return sut.Handle(command, cancellationToken);
        });
        var request = new RootDepartmentsRequest(
            Page: 1,
            Size: 2,
            Prefetch: 3);
        
        // first get
        var firstGetDeps = await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>((sut) =>
        {
            var command = new GetRootDepartmentsQuery(request);

            return sut.Handle(command, cancellationToken);
        });
        
        await Task.Delay(200, cancellationToken);
        
        var secondGetDeps = await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>((sut) =>
        {
            var command = new GetRootDepartmentsQuery(request);

            return sut.Handle(command, cancellationToken);
        });
        
        await Task.Delay(200, cancellationToken);

        var departmentAId = createDepartmentResult.Value;
        var departmentBId = createDepartmentResult2.Value;
        
        await ExecuteHandler<int, UpdateParentHandler>((sut) =>
        {
            var command = new UpdateParentCommand(
                departmentBId,
                new UpdateParentRequest()
                {
                    ParentId = departmentAId,
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        await Task.Delay(200, cancellationToken);
        
        var thirdGetDeps = await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>((sut) =>
        {
            var command = new GetRootDepartmentsQuery(request);

            return sut.Handle(command, cancellationToken);
        });
        await Task.Delay(200, cancellationToken);
        var key = new RootDepartmentsCacheKey(request);
        
        // первый получен из БД, второй из Redis, третий - после инвалидации.
        
        Assert.True(await _factory.RedisDb.KeyExistsAsync(key.Value));
        Assert.Equal(2, firstGetDeps.Value.Departments.Count);
        Assert.Equal(2, secondGetDeps.Value.Departments.Count);
        Assert.Single(thirdGetDeps.Value.Departments);
        
        Assert.Empty(firstGetDeps.Value.Departments[0].Children);
        Assert.Empty(secondGetDeps.Value.Departments[0].Children);
        Assert.Single(thirdGetDeps.Value.Departments[0].Children);
        Assert.Equal(thirdGetDeps.Value.Departments[0].Children[0].Id, departmentBId);
    }
}