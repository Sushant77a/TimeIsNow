using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Notes;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class NotesService(
    INoteRepository notes,
    ITagRepository tags,
    IUnitOfWork unitOfWork) : INotesService
{
    public async Task<IReadOnlyList<NoteDto>> GetListAsync(NoteListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await notes.GetListAsync(
            query.CategoryId, query.Search, query.Tag, query.Pinned, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<NoteDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ToDto(await FindRequiredAsync(id, cancellationToken));

    public async Task<NoteDto> CreateAsync(NoteRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var note = new Note
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Content = request.Content,
            IsPinned = request.IsPinned,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await ApplyTagsAsync(note, request.Tags, cancellationToken);

        notes.Add(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(note);
    }

    public async Task<NoteDto> UpdateAsync(int id, NoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = await FindRequiredAsync(id, cancellationToken);
        note.CategoryId = request.CategoryId;
        note.Title = request.Title;
        note.Content = request.Content;
        note.IsPinned = request.IsPinned;

        await ApplyTagsAsync(note, request.Tags, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(note);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var note = await FindRequiredAsync(id, cancellationToken);
        notes.Remove(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyTagsAsync(Note note, List<string>? tagNames, CancellationToken cancellationToken)
    {
        if (tagNames is null)
            return;

        var resolved = await tags.EnsureManyAsync(tagNames, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        note.Tags.Clear();
        foreach (var tag in resolved)
            note.Tags.Add(new NoteTag { Note = note, Tag = tag });
    }

    private async Task<Note> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await notes.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.NoteNotFound);

    private static NoteDto ToDto(Note note) => new()
    {
        Id = note.Id,
        CategoryId = note.CategoryId,
        Title = note.Title,
        Content = note.Content,
        IsPinned = note.IsPinned,
        Tags = [.. note.Tags.Select(tag => tag.Tag.Name)],
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt,
    };
}
