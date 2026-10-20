using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Habits;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/habits")]
public sealed class HabitsController(IHabitsService habitsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] HabitListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await habitsService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.HabitsFetched));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await habitsService.GetByIdAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.HabitFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(HabitRequest request, CancellationToken cancellationToken = default)
    {
        var result = await habitsService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, ApiMessages.HabitCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, HabitRequest request, CancellationToken cancellationToken = default)
    {
        var result = await habitsService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.HabitUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await habitsService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.HabitDeleted));
    }

    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetLogs(int id, [FromQuery] HabitLogListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await habitsService.GetLogsAsync(id, query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.HabitLogsFetched));
    }

    [HttpPost("{id:int}/logs")]
    public async Task<IActionResult> UpsertLog(int id, HabitLogRequest request, CancellationToken cancellationToken = default)
    {
        var result = await habitsService.UpsertLogAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.HabitLogSaved));
    }

    [HttpDelete("logs/{logId:int}")]
    public async Task<IActionResult> DeleteLog(int logId, CancellationToken cancellationToken = default)
    {
        await habitsService.DeleteLogAsync(logId, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.HabitLogDeleted));
    }
}
