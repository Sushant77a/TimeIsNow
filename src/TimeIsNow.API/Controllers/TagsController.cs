using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/tags")]
public sealed class TagsController(ITagsService tagsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await tagsService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.TagsFetched));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await tagsService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse.Success(ApiMessages.TagDeleted));
    }
}
