using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    void Add(User user);
}
