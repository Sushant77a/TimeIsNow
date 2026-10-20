using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Categories;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ICategoriesService categoriesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var result = await categoriesService.GetAllAsync(includeArchived, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.CategoriesFetched));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await categoriesService.GetByIdAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.CategoryFetched));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await categoriesService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, ApiMessages.CategoryCreated, 201));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await categoriesService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.CategoryUpdated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await categoriesService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.CategoryDeleted));
    }
}
