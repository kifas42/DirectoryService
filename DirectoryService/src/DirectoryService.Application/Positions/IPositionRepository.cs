using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionRepository
{
    public Task<Result<PositionId, Error>> AddAsync(Position position, CancellationToken cancellationToken);
}