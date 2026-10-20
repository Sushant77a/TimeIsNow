using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Infrastructure.Data;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(TimeIsNowDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

    public void Add(RefreshToken token) => context.RefreshTokens.Add(token);

    public void Remove(RefreshToken token) => context.RefreshTokens.Remove(token);

    public void RemoveRange(IEnumerable<RefreshToken> tokens) => context.RefreshTokens.RemoveRange(tokens);
}
