using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(
    IDashboardService dashboardService,
    IWeeklyBriefingService briefingService) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> GetToday([FromQuery] DateOnly? date, CancellationToken cancellationToken = default)
    {
        var result = await dashboardService.GetTodayAsync(date, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.DashboardTodayFetched));
    }

    [HttpGet("briefing")]
    public async Task<IActionResult> GetBriefing([FromQuery] DateOnly? weekStart, CancellationToken cancellationToken = default)
    {
        var result = await briefingService.GetWeeklyBriefingAsync(weekStart, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.DashboardBriefingFetched));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken = default)
    {
        var result = await dashboardService.GetSummaryAsync(year, month, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.DashboardSummaryFetched));
    }
}
