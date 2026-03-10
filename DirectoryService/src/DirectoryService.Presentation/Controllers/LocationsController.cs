using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateLocationRequest locationRequest,
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(locationRequest);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PaginationLocationResponse>> Get(
        [FromQuery] GetLocationRequest locationRequest,
        [FromServices] IQueryHandler<PaginationLocationResponse, GetLocationQuery> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new GetLocationQuery(locationRequest), cancellationToken);
    }
}