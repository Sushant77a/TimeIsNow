using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Journal;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class JournalService(
    IJournalRepository journal,
    ITagRepository tags,
    IUnitOfWork unitOfWork) : IJournalService
{
    public async Task<IReadOnlyList<JournalEntryDto>> GetListAsync(JournalListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await journal.GetListAsync(query.CategoryId, query.From, query.To, query.Search, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<JournalEntryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ToDto(await FindRequiredAsync(id, cancellationToken));

    public async Task<JournalEntryDto> CreateAsync(JournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entry = new JournalEntry
        {
            CategoryId = request.CategoryId,
            Date = request.Date,
            Title = request.Title,
            Content = request.Content,
            Mood = request.Mood,
            Energy = request.Energy,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await ApplyTagsAsync(entry, request.Tags, cancellationToken);

        journal.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(entry);
    }

    public async Task<JournalEntryDto> UpdateAsync(int id, JournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await FindRequiredAsync(id, cancellationToken);
        entry.CategoryId = request.CategoryId;
        entry.Date = request.Date;
        entry.Title = request.Title;
        entry.Content = request.Content;
        entry.Mood = request.Mood;
        entry.Energy = request.Energy;

        await ApplyTagsAsync(entry, request.Tags, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(entry);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entry = await FindRequiredAsync(id, cancellationToken);
        journal.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyTagsAsync(JournalEntry entry, List<string>? tagNames, CancellationToken cancellationToken)
    {
        if (tagNames is null)
            return;

        var resolved = await tags.EnsureManyAsync(tagNames, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        entry.Tags.Clear();
        foreach (var tag in resolved)
            entry.Tags.Add(new JournalTag { JournalEntry = entry, Tag = tag });
    }

    private async Task<JournalEntry> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await journal.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.JournalEntryNotFound);

    private static JournalEntryDto ToDto(JournalEntry entry) => new()
    {
        Id = entry.Id,
        CategoryId = entry.CategoryId,
        Date = entry.Date,
        Title = entry.Title,
        Content = entry.Content,
        Mood = entry.Mood,
        Energy = entry.Energy,
        Tags = [.. entry.Tags.Select(tag => tag.Tag.Name)],
        CreatedAt = entry.CreatedAt,
        UpdatedAt = entry.UpdatedAt,
    };
}
