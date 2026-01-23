using DirectoryService.Application.Abstractions;
using DirectoryService.Application.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<LocationId>> Create(
        [FromBody] CreateLocationRequest locationRequest,
        [FromServices] ICommandHandler<LocationId, CreateLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(locationRequest);
        return await handler.Handle(command, cancellationToken);
    }
}