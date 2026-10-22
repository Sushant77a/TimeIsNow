using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Practices;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/practices")]
public sealed class PracticesController(
    IPracticesService practicesService,
    IPracticePlanGenerator planGenerator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PracticeListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await practicesService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.PracticesFetched));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await practicesService.GetByIdAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.PracticeFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(PracticeUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await practicesService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, ApiMessages.PracticeCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PracticeUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await practicesService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.PracticeUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await practicesService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.PracticeDeleted));
    }

    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetLogs(int id, [FromQuery] PracticeLogListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await practicesService.GetLogsAsync(id, query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.PracticeLogsFetched));
    }

    [HttpPost("{id:int}/logs")]
    public async Task<IActionResult> UpsertLog(int id, PracticeLogRequest request, CancellationToken cancellationToken = default)
    {
        var result = await practicesService.UpsertLogAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.PracticeLogSaved));
    }

    [HttpDelete("logs/{logId:int}")]
    public async Task<IActionResult> DeleteLog(int logId, CancellationToken cancellationToken = default)
    {
        await practicesService.DeleteLogAsync(logId, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.PracticeLogDeleted));
    }

    [HttpPost("plan")]
    public async Task<IActionResult> GeneratePlan(PracticePlanRequest request, CancellationToken cancellationToken = default)
    {
        var result = await planGenerator.GenerateAsync(request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.PracticePlanGenerated));
    }
}
