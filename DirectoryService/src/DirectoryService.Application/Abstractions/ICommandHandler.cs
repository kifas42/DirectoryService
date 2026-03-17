using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Application.Abstractions;

public interface ICommand;

public interface ICommandHandler;

public interface ICommandHandler<TResponse, in TCommand> : ICommandHandler
    where TCommand : ICommand
{
    Task<Result<TResponse, Error>> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand> : ICommandHandler
    where TCommand : ICommand
{
    Task<UnitResult<Error>> Handle(TCommand command, CancellationToken cancellationToken = default);
}