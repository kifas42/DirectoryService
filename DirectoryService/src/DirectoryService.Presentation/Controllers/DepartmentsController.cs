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
        CreateDepartmentCommand command = new(departmentRequest);
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
        UpdateLocationCommand command = new(departmentId, updateLocationsRequest);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut]
    [Route("{departmentId:guid}/parent")]
    public async Task<EndpointResult<int>> UpdateParent(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateParentRequest updateParentRequest,
        [FromServices] ICommandHandler<int, UpdateParentCommand> handler,
        CancellationToken cancellationToken)
    {
        UpdateParentCommand command = new(departmentId, updateParentRequest);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    [Route("")]
    public async Task<EndpointResult<DepartmentsResponse>> Get(
        [FromQuery] GetDepartmentsRequest getDepartmentsRequest,
        [FromServices] IQueryHandler<DepartmentsResponse, GetDepartmentsQuery> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new GetDepartmentsQuery(getDepartmentsRequest), cancellationToken);

    [HttpGet]
    [Route("top-positions")]
    public async Task<EndpointResult<TopDepartmentsResponse>> GetTopPositions(
        [FromQuery] int? count,
        [FromServices] IQueryHandler<TopDepartmentsResponse, GetTopDepartmentsQuery> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new GetTopDepartmentsQuery(count ?? 5), cancellationToken);

    [HttpGet]
    [Route("roots")]
    public async Task<EndpointResult<DepartmentsResponse>> GetRoots(
        [FromQuery] RootDepartmentsRequest rootDepartmentsRequest,
        [FromServices] IQueryHandler<DepartmentsResponse, GetRootDepartmentsQuery> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new GetRootDepartmentsQuery(rootDepartmentsRequest), cancellationToken);

    [HttpGet]
    [Route("{departmentId:guid}/children")]
    public async Task<EndpointResult<DepartmentsResponse>> GetChildren(
        [FromQuery] ChildDepartmentsRequest departmentsRequest,
        [FromRoute] Guid departmentId,
        [FromServices] IQueryHandler<DepartmentsResponse, GetChildDepartmentsQuery> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new GetChildDepartmentsQuery(departmentId, departmentsRequest), cancellationToken);

    [HttpDelete]
    [Route("{departmentId:guid}")]
    public async Task<EndpointResult> Delete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<DeleteDepartmentCommand> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new DeleteDepartmentCommand(departmentId), cancellationToken);
}