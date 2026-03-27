using CSharpFunctionalExtensions;

using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    protected IServiceProvider Services { get; }
    private readonly Func<Task> _resetDatabase;

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<Result<T, Error>> ExecuteHandler<T, THandler>(Func<THandler, Task<Result<T, Error>>> action)
        where THandler : class
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<THandler>();

        return await action(sut);
    }

    protected async Task<T> ExecuteInDb<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<ApplicationDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await action(dbContext);
    }
}