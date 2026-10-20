using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Todos;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/todos")]
public sealed class TodosController(ITodosService todosService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] TodoListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await todosService.GetListAsync(query, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.TodosFetched));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await todosService.GetByIdAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.TodoFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TodoRequest request, CancellationToken cancellationToken = default)
    {
        var result = await todosService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, ApiMessages.TodoCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TodoRequest request, CancellationToken cancellationToken = default)
    {
        var result = await todosService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.TodoUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await todosService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.TodoDeleted));
    }
}
