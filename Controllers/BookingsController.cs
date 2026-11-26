using BookingApi.Dtos;
using BookingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/v1/bookings")]
public class BookingsController(ScheduleService scheduleService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var created = await scheduleService.CreateBookingAsync(request, ct);
        return Created($"/api/v1/bookings/{created.Id}", created);
    }

    [HttpGet]
    public async Task<ActionResult<List<BookingResponse>>> GetAll([FromQuery] int? resourceId, CancellationToken ct)
    {
        var list = await scheduleService.GetBookingsAsync(resourceId, ct);
        return Ok(list);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<BookingResponse>> Cancel([FromRoute] int id, CancellationToken ct)
    {
        var cancelled = await scheduleService.CancelBookingAsync(id, ct);
        return Ok(cancelled);
    }
}
