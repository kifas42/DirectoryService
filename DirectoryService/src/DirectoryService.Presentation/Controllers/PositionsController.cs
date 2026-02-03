using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreatePositionRequest positionRequest,
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(positionRequest);
        return await handler.Handle(command, cancellationToken);
    }
}