using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Schedule;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/schedule")]
public sealed class ScheduleController(IScheduleService scheduleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ScheduleListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await scheduleService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.ScheduleFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ScheduleItemRequest request, CancellationToken cancellationToken = default)
    {
        var result = await scheduleService.CreateAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse.Success(result, ApiMessages.ScheduleItemCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ScheduleItemRequest request, CancellationToken cancellationToken = default)
    {
        var result = await scheduleService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.ScheduleItemUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await scheduleService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.ScheduleItemDeleted));
    }
}
