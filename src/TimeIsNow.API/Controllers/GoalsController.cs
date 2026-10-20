using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Goals;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/goals")]
public sealed class GoalsController(IGoalsService goalsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GoalListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await goalsService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.GoalsFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(GoalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await goalsService.CreateAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse.Success(result, ApiMessages.GoalCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GoalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await goalsService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.GoalUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await goalsService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.GoalDeleted));
    }
}
