using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateDepartmentRequest departmentRequest,
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(departmentRequest);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut]
    [Route("{departmentId:guid}/locations")]
    public async Task<EndpointResult<int>> UpdateLocations(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateLocationsRequest updateLocationsRequest,
        [FromServices] ICommandHandler<int, UpdateLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(departmentId, updateLocationsRequest);
        return await handler.Handle(command, cancellationToken);
    }
}