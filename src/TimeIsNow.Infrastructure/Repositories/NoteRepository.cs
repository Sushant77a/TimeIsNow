using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class NoteRepository(TimeIsNowDbContext context) : INoteRepository
{
    public async Task<IReadOnlyList<Note>> GetListAsync(
        int? categoryId, string? search, string? tag, bool? pinned,
        CancellationToken cancellationToken = default)
    {
        var query = context.Notes
            .Include(note => note.Tags).ThenInclude(noteTag => noteTag.Tag)
            .Where(note => !note.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(note => note.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(note =>
                note.Title.Contains(search) || note.Content.Contains(search));

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(note =>
                note.Tags.Any(noteTag => noteTag.Tag.Name == tag));

        if (pinned.HasValue)
            query = query.Where(note => note.IsPinned == pinned.Value);

        return await query
            .OrderByDescending(note => note.IsPinned)
            .ThenByDescending(note => note.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Note?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Notes
            .Include(note => note.Tags).ThenInclude(noteTag => noteTag.Tag)
            .FirstOrDefaultAsync(note => note.Id == id && !note.IsDeleted, cancellationToken);

    public void Add(Note note) => context.Notes.Add(note);

    public void Remove(Note note) => note.IsDeleted = true;
}
