using TimeIsNow.Application.Features.Profile;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileDto> GetAsync(CancellationToken cancellationToken = default);
    Task<ProfileDto> UpdateAsync(ProfileRequest request, CancellationToken cancellationToken = default);
}
