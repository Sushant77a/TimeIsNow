using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Infrastructure.Data;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class UserRepository(TimeIsNowDbContext context) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public void Add(User user) => context.Users.Add(user);
}
