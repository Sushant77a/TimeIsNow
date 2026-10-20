using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Profile;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class ProfileService(
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork) : IProfileService
{
    public async Task<ProfileDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var profile = await GetRequiredAsync(cancellationToken);
        return ToDto(profile);
    }

    public async Task<ProfileDto> UpdateAsync(ProfileRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await GetRequiredAsync(cancellationToken);
        profile.DisplayName = request.DisplayName;
        profile.Currency = request.Currency;
        profile.WeekStartsOn = request.WeekStartsOn;
        profile.Theme = request.Theme;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(profile);
    }

    private async Task<UserProfile> GetRequiredAsync(CancellationToken cancellationToken) =>
        await profiles.GetAsync(cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.ProfileNotFound);

    private static ProfileDto ToDto(UserProfile profile) => new()
    {
        Id = profile.Id,
        DisplayName = profile.DisplayName,
        Currency = profile.Currency,
        WeekStartsOn = profile.WeekStartsOn,
        Theme = profile.Theme,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt,
    };
}
