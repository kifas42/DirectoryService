using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Application.Abstractions;

public interface IQuery;

public interface IQueryHandler;

public interface IQueryHandler<TResponse, in TQuery> : IQueryHandler
    where TQuery : IQuery
{
    Task<Result<TResponse, Error>> Handle(TQuery query, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<in TQuery> : IQueryHandler
    where TQuery : IQuery
{
    Task<UnitResult<Error>> Handle(TQuery query, CancellationToken cancellationToken = default);
}