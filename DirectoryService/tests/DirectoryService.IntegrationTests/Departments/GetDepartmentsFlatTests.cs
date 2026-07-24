using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentsFlatTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    private readonly Guid _engId = Guid.NewGuid();
    private readonly Guid _hrId = Guid.NewGuid();
    private readonly Guid _itId = Guid.NewGuid();
    private readonly Guid _salesId = Guid.NewGuid();

    [Fact]
    public async Task GetDepartments_WithFiltersAndPagination_ShouldReturnCorrectFlatList()
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

        Result<DepartmentsResponse, Error> searchResult =
            await ExecuteHandler<DepartmentsResponse, GetDepartmentsHandler>(sut =>
            {
                // Ищем "SALES" в верхнем регистре, чтобы проверить ToLower() / ILIKE
                GetDepartmentsQuery query =
                    new(new GetDepartmentsRequest { Search = "SALES", Page = 1, PageSize = 10, });

                return sut.Handle(query, cancellationToken);
            });

        Result<DepartmentsResponse, Error> paginationResult =
            await ExecuteHandler<DepartmentsResponse, GetDepartmentsHandler>(sut =>
            {
                GetDepartmentsQuery query = new(new GetDepartmentsRequest
                {
                    Page = 1, PageSize = 2, // Проверяем, что вернется не более 2 элементов
                });

                return sut.Handle(query, cancellationToken);
            });

        // 4. Тест сценария 3: Исключение определенных ID (ExcludeIds)
        Result<DepartmentsResponse, Error> excludeResult =
            await ExecuteHandler<DepartmentsResponse, GetDepartmentsHandler>(sut =>
            {
                GetDepartmentsQuery query = new(new GetDepartmentsRequest
                {
                    ExcludeIds = [_salesId], // Исключаем корневой департамент продаж
                    Page = 1,
                    PageSize = 50,
                });

                return sut.Handle(query, cancellationToken);
            });

        Assert.True(searchResult.IsSuccess);
        Assert.NotNull(searchResult.Value);
        Assert.NotEmpty(searchResult.Value.Departments);

        // Проверяем, что все вернувшиеся департаменты содержат подстроку "sales"
        Assert.All(searchResult.Value.Departments, d =>
            Assert.Contains("sales", d.Name, StringComparison.OrdinalIgnoreCase));

        // Assert для пагинации
        Assert.True(paginationResult.IsSuccess);
        Assert.NotNull(paginationResult.Value);
        Assert.Equal(2, paginationResult.Value.Departments.Count);
        Assert.True(paginationResult.Value.TotalCount > 2); // Общее количество под фильтром больше, чем размер страницы

        // Assert для исключения ID
        Assert.True(excludeResult.IsSuccess);
        Assert.NotNull(excludeResult.Value);

        // Проверяем, что исключенного ID нет в результирующем плоском списке
        Assert.DoesNotContain(excludeResult.Value.Departments, d => d.Id == _salesId);
    }

    [Fact]
    public async Task GetDepartments_WithInvalidPageSize_ShouldReturnValidationError()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        // Тест сценария 4: Проверка валидации (Невалидный PageSize)
        Result<DepartmentsResponse, Error> result =
            await ExecuteHandler<DepartmentsResponse, GetDepartmentsHandler>(sut =>
            {
                GetDepartmentsQuery query = new(new GetDepartmentsRequest
                {
                    Page = 1, PageSize = 101, // Больше максимального лимита в 100
                });

                return sut.Handle(query, cancellationToken);
            });

        // Проверяем, что хендлер вернул ошибку валидации через Envelope/Result, не упав с исключением
        Assert.True(result.IsFailure);
        Assert.Equal(SharedErrorCodes.Validation.InvalidRequest, result.Error.Messages[0].Code);
    }
}