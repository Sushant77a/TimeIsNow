using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Notes;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/notes")]
public sealed class NotesController(INotesService notesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] NoteListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await notesService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.NotesFetched));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await notesService.GetByIdAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.NoteFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(NoteRequest request, CancellationToken cancellationToken = default)
    {
        var result = await notesService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, ApiMessages.NoteCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, NoteRequest request, CancellationToken cancellationToken = default)
    {
        var result = await notesService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.NoteUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await notesService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.NoteDeleted));
    }
}
