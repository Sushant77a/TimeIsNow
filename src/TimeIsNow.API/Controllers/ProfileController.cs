using Microsoft.AspNetCore.Mvc;
using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.ResponseWrappers;
using TimeIsNow.Application.Features.Profile;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.API.Controllers;

[ApiController]
[Route("api/profile")]
public sealed class ProfileController(IProfileService profileService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var result = await profileService.GetAsync(cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.ProfileFetched));
    }

    [HttpPut]
    public async Task<IActionResult> Update(ProfileRequest request, CancellationToken cancellationToken = default)
    {
        var result = await profileService.UpdateAsync(request, cancellationToken);

        return Ok(ApiResponse.Success(result, ApiMessages.ProfileUpdated));
    }
}
