using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations;

public record DeleteLocationCommand(Guid Id) : ICommand;

public class SoftDeleteLocationHandler : ICommandHandler<DeleteLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<SoftDeleteLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public SoftDeleteLocationHandler(
        ILocationRepository locationRepository,
        ILogger<SoftDeleteLocationHandler> logger,
        ITransactionManager transactionManager)
    {
        _locationRepository = locationRepository;
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var locationId = new LocationId(command.Id);


        var locationResult = await _locationRepository.GetAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            _logger.LogError("Failed to get location: {ErrorMessage}", locationResult.Error);
            return locationResult.Error;
        }

        var deleteResult = locationResult.Value.SoftDelete();
        if (deleteResult.IsFailure)
        {
            _logger.LogError("Failed to delete location: {ErrorMessage}", deleteResult.Error);
            return deleteResult.Error;
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}