using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.DailyEntries;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/daily-entries")]
public sealed class DailyEntriesController(IDailyEntriesService dailyEntriesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] DailyEntryListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await dailyEntriesService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.DailyEntriesFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(DailyEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await dailyEntriesService.CreateAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse.Success(result, ApiMessages.DailyEntryCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DailyEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await dailyEntriesService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.DailyEntryUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await dailyEntriesService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.DailyEntryDeleted));
    }
}
