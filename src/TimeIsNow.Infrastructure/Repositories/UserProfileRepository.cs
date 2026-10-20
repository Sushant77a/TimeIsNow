using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class UserProfileRepository(TimeIsNowDbContext context) : IUserProfileRepository
{
    public Task<UserProfile?> GetAsync(CancellationToken cancellationToken = default) =>
        context.UserProfiles.OrderBy(profile => profile.Id).FirstOrDefaultAsync(cancellationToken);

    public void Add(UserProfile profile) => context.UserProfiles.Add(profile);
}
