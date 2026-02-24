using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateLocationsTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task UpdateLocations_WithValidData_ShouldSucceed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId = null;
        LocationId? newLocationId = null;
        DepartmentId? departmentId = null;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
            newLocationId = await DataCreator.CreateLocation(dbContext, "Business Center", "BBB");
            departmentId = await DataCreator.CreateDepartment(dbContext, locationId);
        });

        // act

        var result = await ExecuteHandler<int, UpdateLocationsHandler>((sut) =>
        {
            var command = new UpdateLocationCommand(
                departmentId!.Value,
                new UpdateLocationsRequest()
                {
                    LocationIds = [newLocationId!.Value]
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var departmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == newLocationId,
                    cancellationToken);

            var oldDepartmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                    cancellationToken);

            Assert.NotNull(departmentLocations);
            Assert.Null(oldDepartmentLocations);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(0, result.Value);
        });
    }

    [Fact]
    public async Task UpdateLocations_WithEmptyLocations_ShouldFailed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId = null;
        DepartmentId? departmentId = null;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
            departmentId = await DataCreator.CreateDepartment(dbContext, locationId);
        });

        // act

        var result = await ExecuteHandler<int, UpdateLocationsHandler>((sut) =>
        {
            var command = new UpdateLocationCommand(
                departmentId!.Value,
                new UpdateLocationsRequest()
                {
                    LocationIds = []
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var oldDepartmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                    cancellationToken);

            Assert.NotNull(oldDepartmentLocations);
            Assert.True(result.IsFailure);
            Assert.Single(result.Error);
            Assert.Equal(ErrorType.VALIDATION, result.Error.First().Type);
        });
    }

    [Fact]
    public async Task UpdateLocations_WithDuplicateLocations_ShouldFailed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId = null;
        LocationId? newLocationId = null;
        DepartmentId? departmentId = null;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
            newLocationId = await DataCreator.CreateLocation(dbContext, "Business Center", "BBB");
            departmentId = await DataCreator.CreateDepartment(dbContext, locationId);
        });

        // act

        var result = await ExecuteHandler<int, UpdateLocationsHandler>((sut) =>
        {
            var command = new UpdateLocationCommand(
                departmentId!.Value,
                new UpdateLocationsRequest()
                {
                    LocationIds = [newLocationId!.Value, newLocationId.Value]
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var oldDepartmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                    cancellationToken);

            var departmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == newLocationId,
                    cancellationToken);

            Assert.NotNull(oldDepartmentLocations);
            Assert.Null(departmentLocations);
            Assert.True(result.IsFailure);
            Assert.Equal(2, result.Error.Count());
        });
    }

    [Fact]
    public async Task UpdateLocations_OnNonExistDepartment_ShouldFailed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId = null;
        LocationId? newLocationId = null;
        DepartmentId? departmentId = DepartmentId.New();

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
            newLocationId = await DataCreator.CreateLocation(dbContext, "Business Center", "BBB");
        });

        // act

        var result = await ExecuteHandler<int, UpdateLocationsHandler>((sut) =>
        {
            var command = new UpdateLocationCommand(
                departmentId!.Value,
                new UpdateLocationsRequest()
                {
                    LocationIds = [newLocationId!.Value]
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var oldDepartmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                    cancellationToken);

            var departmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == newLocationId,
                    cancellationToken);

            Assert.Null(oldDepartmentLocations);
            Assert.Null(departmentLocations);
            Assert.True(result.IsFailure);
            Assert.Single(result.Error);
            Assert.Equal(ErrorType.NOT_FOUND, result.Error.First().Type);
        });
    }


    [Fact]
    public async Task UpdateLocations_WithNonExistLocation_ShouldFailed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;
        LocationId? locationId = null;
        LocationId? newLocationId = LocationId.New();
        DepartmentId? departmentId = null;
        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
            departmentId = await DataCreator.CreateDepartment(dbContext, locationId);
        });

        // act

        var result = await ExecuteHandler<int, UpdateLocationsHandler>((sut) =>
        {
            var command = new UpdateLocationCommand(
                departmentId!.Value,
                new UpdateLocationsRequest()
                {
                    LocationIds = [newLocationId!.Value]
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var oldDepartmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                    cancellationToken);
            var departmentLocations = await dbContext.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == newLocationId,
                    cancellationToken);

            Assert.NotNull(oldDepartmentLocations);
            Assert.Null(departmentLocations);
            Assert.True(result.IsFailure);
            Assert.Single(result.Error);
            Assert.Equal(ErrorType.NOT_FOUND, result.Error.First().Type);
        });
    }
}