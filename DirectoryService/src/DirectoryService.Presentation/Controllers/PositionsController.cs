using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<PositionId>> Create(
        [FromBody] CreatePositionRequest positionRequest,
        [FromServices] ICommandHandler<PositionId, CreatePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(positionRequest);
        return await handler.Handle(command, cancellationToken);
    }
}