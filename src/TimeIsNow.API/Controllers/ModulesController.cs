using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Modules;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/modules")]
public sealed class ModulesController(IModulesService modulesService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<IActionResult> GetAllScope(CancellationToken cancellationToken = default)
    {
        var result = await modulesService.GetAllScopeModulesAsync(cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.ModulesFetched));
    }

    [HttpPut("all")]
    public async Task<IActionResult> UpdateAllScope(UpdateModulesRequest request, CancellationToken cancellationToken = default)
    {
        var result = await modulesService.UpdateAllScopeModulesAsync(request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.ModulesUpdated));
    }

    [HttpPut("category/{categoryId:int}")]
    public async Task<IActionResult> UpdateCategoryScope(int categoryId, UpdateModulesRequest request, CancellationToken cancellationToken = default)
    {
        var result = await modulesService.UpdateCategoryModulesAsync(categoryId, request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.ModulesUpdated));
    }
}
