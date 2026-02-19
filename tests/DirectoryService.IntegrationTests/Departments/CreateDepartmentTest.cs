using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTest : DirectoryBaseTests
{
    public CreateDepartmentTest(DirectoryTestWebFactory factory) : base(factory)
    {
    }


    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // arrange

        var locationId = await CreateLocation();
        var cancellationToken = CancellationToken.None;

        // act

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest()
                {
                    Name = "Подразделение",
                    Identifier = "dep",
                    ParentId = null,
                    LocationIds = [locationId.Value],
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
    public async Task CreateDepartment_with_invalid_location_should_failed()
    {
        // arrange
        var locationId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // act

        var result = await ExecuteHandler((sut) =>
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

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    private async Task<LocationId> CreateLocation()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                "ЛОХация",
                Address.Create(
                    "111",
                    "b222",
                    "street",
                    "NightCty",
                    "Ohio",
                    "country",
                    "11223").Value,
                Timezone.Create("Europe/London").Value
            );
            dbContext.Locations.Add(location.Value);
            await dbContext.SaveChangesAsync();
            return location.Value.Id;
        });
    }
}