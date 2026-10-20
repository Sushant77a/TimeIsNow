using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class TagRepository(TimeIsNowDbContext context) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Tags.OrderBy(tag => tag.Name).ToListAsync(cancellationToken);

    public Task<Tag?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Tags.FirstOrDefaultAsync(tag => tag.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Tag>> EnsureManyAsync(IReadOnlyList<string> names, CancellationToken cancellationToken = default)
    {
        var normalized = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
            return [];

        var existing = await context.Tags
            .Where(tag => normalized.Contains(tag.Name))
            .ToListAsync(cancellationToken);

        var result = new List<Tag>(existing);
        var now = DateTime.UtcNow;

        foreach (var name in normalized.Where(name => existing.All(tag => tag.Name != name)))
        {
            var tag = new Tag { Name = name, CreatedAt = now };
            context.Tags.Add(tag);
            result.Add(tag);
        }

        return result
            .OrderBy(tag => Array.IndexOf(normalized.ToArray(), tag.Name))
            .ToList();
    }

    public void Add(Tag tag) => context.Tags.Add(tag);

    public void Remove(Tag tag) => context.Tags.Remove(tag);
}
