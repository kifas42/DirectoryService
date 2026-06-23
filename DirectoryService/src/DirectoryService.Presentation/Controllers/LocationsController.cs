using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts;
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
        CreateLocationCommand command = new(locationRequest);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PaginationResponse<GetLocationDto>>> Get(
        [FromQuery] GetLocationRequest locationRequest,
        [FromServices] IQueryHandler<PaginationResponse<GetLocationDto>, GetLocationQuery> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new GetLocationQuery(locationRequest), cancellationToken);
}