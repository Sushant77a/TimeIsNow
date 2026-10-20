using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(int userId, CancellationToken cancellationToken = default);
    void Add(RefreshToken token);
    void Remove(RefreshToken token);
    void RemoveRange(IEnumerable<RefreshToken> tokens);
}
