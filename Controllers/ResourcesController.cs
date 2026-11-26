using BookingApi.Dtos;
using BookingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/v1/resources")]
public class ResourcesController(ScheduleService scheduleService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ResourceResponse>> Create([FromBody] CreateResourceRequest request, CancellationToken ct)
    {
        var created = await scheduleService.CreateResourceAsync(request, ct);
        return Created($"/api/v1/resources/{created.Id}", created);
    }

    [HttpGet]
    public async Task<ActionResult<List<ResourceResponse>>> GetAll(CancellationToken ct)
    {
        var list = await scheduleService.GetResourcesAsync(ct);
        return Ok(list);
    }
}
