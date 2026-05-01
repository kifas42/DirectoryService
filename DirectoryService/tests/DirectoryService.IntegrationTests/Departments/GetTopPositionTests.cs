using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

public class GetTopPositionTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task GetTopDepartments_WithValidData_ShouldReturnTopDepartments()
    {
        LocationId? locationId;
        var cancellationToken = CancellationToken.None;
        List<TestPositionDto> positions =
        [
            new(Guid.NewGuid(), "AAA"),
            new(Guid.NewGuid(), "BBB"),
            new(Guid.NewGuid(), "CCC"),
            new(Guid.NewGuid(), "DDD"),
            new(Guid.NewGuid(), "EEE"),
            new(Guid.NewGuid(), "FFF"),
            new(Guid.NewGuid(), "GGG"),
            new(Guid.NewGuid(), "HHH"),
            new(Guid.NewGuid(), "III"),
            new(Guid.NewGuid(), "JJJ"),
        ];

        List<PositionId> positionIds;

        await ExecuteInDb(async dbContext =>
        {
            locationId = await DataCreator.CreateLocation(dbContext);
            positionIds = (await DataCreator.CreatePositions(dbContext, positions, cancellationToken)).ToList();

            for (var i = 0; i < 10; i++)
            {
                await DataCreator.CreateDepartmentNoSave(
                    dbContext,
                    [locationId],
                    positionIds.Take(i + 1).ToList(),
                    $"DEPARTMENT {positions[i].Name}",
                    $"dep-{positions[i].Name.ToLower()}",
                    null,
                    null);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        });


        var result5 = await ExecuteHandler<TopDepartmentsResponse, GetTopDepartmentsHandler>((sut) =>
        {
            var command = new GetTopDepartmentsQuery(5);

            return sut.Handle(command, cancellationToken);
        });

        var result8 = await ExecuteHandler<TopDepartmentsResponse, GetTopDepartmentsHandler>((sut) =>
        {
            var command = new GetTopDepartmentsQuery(8);

            return sut.Handle(command, cancellationToken);
        });

        var result1 = await ExecuteHandler<TopDepartmentsResponse, GetTopDepartmentsHandler>((sut) =>
        {
            var command = new GetTopDepartmentsQuery(1);

            return sut.Handle(command, cancellationToken);
        });


        Assert.NotNull(result5.Value);
        Assert.Equal(5, result5.Value.Count);
        Assert.Equal(10, result5.Value.TopDepartments[0].PositionsCount);

        Assert.NotNull(result8.Value);
        Assert.Equal(8, result8.Value.Count);
        Assert.Equal(3, result8.Value.TopDepartments[7].PositionsCount);

        Assert.NotNull(result1.Value);
        Assert.Equal(1, result1.Value.Count);
    }
}