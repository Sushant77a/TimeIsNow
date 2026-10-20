using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Finance;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/finance")]
public sealed class FinanceController(IFinanceService financeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] FinanceListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await financeService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.FinanceFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(FinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await financeService.CreateAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse.Success(result, ApiMessages.FinanceEntryCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await financeService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.FinanceEntryUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await financeService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.FinanceEntryDeleted));
    }
}
