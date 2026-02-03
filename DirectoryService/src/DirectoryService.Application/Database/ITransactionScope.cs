using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Application.Database;

public interface ITransactionScope : IDisposable
{
    public UnitResult<Error> Commit();

    public UnitResult<Error> Rollback();
}