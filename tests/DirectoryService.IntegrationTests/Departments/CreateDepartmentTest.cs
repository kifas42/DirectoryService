using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTest(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task CreateDepartment_WithValidData_ShouldSucceed()
    {
        // arrange
        LocationId? locationId =  null;
        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
        });
        var cancellationToken = CancellationToken.None;

        // act

        var result = await ExecuteHandler<Guid, CreateDepartmentHandler>((sut) =>
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

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
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
        var locationId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // act

        var result = await ExecuteHandler<Guid, CreateDepartmentHandler>((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest()
                {
                    Name = "Подразделение",
                    Identifier = "dep",
                    ParentId = null,
                    LocationIds = [locationId],
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        var checkDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.DepartmentLocations.AnyAsync(
                dl => dl.LocationId == new LocationId(locationId),
                cancellationToken: cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
        Assert.False(checkDb);
    }

    [Fact]
    public async Task CreateDepartment_WithEmptyData_ShouldFailed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        // act

        var result = await ExecuteHandler<Guid, CreateDepartmentHandler>((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest()
                {
                    Name = "",
                    Identifier = "",
                    ParentId = null,
                    LocationIds = [],
                }
            );

            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
        Assert.Equal(4, result.Error.Count());
    }
}