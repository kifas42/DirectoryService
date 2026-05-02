using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task CreateDepartment_WithValidData_ShouldSucceed()
    {
        // arrange
        LocationId? locationId = null;
        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
        });
        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Error> result = await ExecuteHandler<Guid, CreateDepartmentHandler>(sut =>
        {
            CreateDepartmentCommand command = new(
                new CreateDepartmentRequest
                {
                    Name = "Подразделение", Identifier = "dep", ParentId = null, LocationIds = [locationId!.Value],
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await ExecuteInDb(async dbContext =>
        {
            Department department = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_WithInvalidLocation_ShouldFailed()
    {
        // arrange
        Guid locationId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Error> result = await ExecuteHandler<Guid, CreateDepartmentHandler>(sut =>
        {
            CreateDepartmentCommand command = new(
                new CreateDepartmentRequest
                {
                    Name = "Подразделение", Identifier = "dep", ParentId = null, LocationIds = [locationId],
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        bool checkDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.DepartmentLocations.AnyAsync(
                dl => dl.LocationId == new LocationId(locationId),
                cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error.Messages);
        Assert.False(checkDb);
    }

    [Fact]
    public async Task CreateDepartment_WithEmptyData_ShouldFailed()
    {
        // arrange
        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Error> result = await ExecuteHandler<Guid, CreateDepartmentHandler>(sut =>
        {
            CreateDepartmentCommand command = new(
                new CreateDepartmentRequest { Name = string.Empty, Identifier = string.Empty, ParentId = null, LocationIds = [] }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
        Assert.Equal(4, result.Error.Messages.Count);
    }
}