using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetAsync(CancellationToken cancellationToken = default);
    void Add(UserProfile profile);
}
