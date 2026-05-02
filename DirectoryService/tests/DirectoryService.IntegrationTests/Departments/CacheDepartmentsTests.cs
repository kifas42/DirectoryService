using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class CacheDepartmentsTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    private readonly DirectoryTestWebFactory _factory = factory;

    [Fact]
    public async Task GetDepartments_WhenCacheMiss_ShouldWriteToRedis()
    {
        LocationId? locationId = null;
        await ExecuteInDb(async dbContext => { locationId = await DataCreator.CreateLocation(dbContext); });
        CancellationToken cancellationToken = CancellationToken.None;
        await ExecuteHandler<Guid, CreateDepartmentHandler>(sut =>
        {
            CreateDepartmentCommand command = new(
                new CreateDepartmentRequest
                {
                    Name = "Подразделение", Identifier = "dep", ParentId = null, LocationIds = [locationId!.Value],
                }
            );

            return sut.Handle(command, cancellationToken);
        });
        RootDepartmentsRequest request = new(
            1,
            2,
            2);
        await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>(sut =>
        {
            GetRootDepartmentsQuery command = new(request);

            return sut.Handle(command, cancellationToken);
        });

        await Task.Delay(300, cancellationToken);

        RootDepartmentsCacheKey key = new(request);
        Assert.True(await _factory.RedisDb.KeyExistsAsync(key.Value));
        Assert.True(await _factory.RedisDb.KeyTimeToLiveAsync(key.Value) > TimeSpan.Zero);
    }

    [Fact]
    public async Task UpdateDepartments_CacheShouldInvalidated()
    {
        LocationId? locationId = null;
        await ExecuteInDb(async dbContext => { locationId = await DataCreator.CreateLocation(dbContext); });
        CancellationToken cancellationToken = CancellationToken.None;
        Result<Guid, Error> createDepartmentResult = await ExecuteHandler<Guid, CreateDepartmentHandler>(sut =>
        {
            CreateDepartmentCommand command = new(
                new CreateDepartmentRequest
                {
                    Name = "Подразделение", Identifier = "dep-a", ParentId = null, LocationIds = [locationId!.Value],
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        Result<Guid, Error> createDepartmentResult2 = await ExecuteHandler<Guid, CreateDepartmentHandler>(sut =>
        {
            CreateDepartmentCommand command = new(
                new CreateDepartmentRequest
                {
                    Name = "Подразделение2",
                    Identifier = "dep-b",
                    ParentId = null,
                    LocationIds = [locationId!.Value],
                }
            );

            return sut.Handle(command, cancellationToken);
        });
        RootDepartmentsRequest request = new(
            1,
            2);

        // first get
        Result<DepartmentsResponse, Error> firstGetDeps =
            await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>(sut =>
            {
                GetRootDepartmentsQuery command = new(request);

                return sut.Handle(command, cancellationToken);
            });

        await Task.Delay(200, cancellationToken);

        Result<DepartmentsResponse, Error> secondGetDeps =
            await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>(sut =>
            {
                GetRootDepartmentsQuery command = new(request);

                return sut.Handle(command, cancellationToken);
            });

        await Task.Delay(200, cancellationToken);

        Guid departmentAId = createDepartmentResult.Value;
        Guid departmentBId = createDepartmentResult2.Value;

        await ExecuteHandler<int, UpdateParentHandler>(sut =>
        {
            UpdateParentCommand command = new(
                departmentBId,
                new UpdateParentRequest { ParentId = departmentAId }
            );

            return sut.Handle(command, cancellationToken);
        });

        await Task.Delay(200, cancellationToken);

        Result<DepartmentsResponse, Error> thirdGetDeps =
            await ExecuteHandler<DepartmentsResponse, GetRootDepartmentsHandler>(sut =>
            {
                GetRootDepartmentsQuery command = new(request);

                return sut.Handle(command, cancellationToken);
            });
        await Task.Delay(200, cancellationToken);
        RootDepartmentsCacheKey key = new(request);

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