using DirectoryService.Application;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateLocationRequest location,
        [FromServices] CreateLocationHandler handler)
    {
        var handleResult = await handler.Handle(location);

        if (!handleResult.IsSuccess)
        {
            return BadRequest(handleResult.Error);
        }

        return Ok(handleResult.Value);
    }
}