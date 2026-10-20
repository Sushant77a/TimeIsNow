using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Journal;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/journal")]
public sealed class JournalController(IJournalService journalService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] JournalListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await journalService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.JournalFetched));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await journalService.GetByIdAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.JournalEntryFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(JournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await journalService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, ApiMessages.JournalEntryCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, JournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await journalService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.JournalEntryUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await journalService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.JournalEntryDeleted));
    }
}
