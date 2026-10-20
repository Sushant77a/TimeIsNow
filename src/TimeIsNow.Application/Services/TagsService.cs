using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Tags;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class TagsService(
    ITagRepository tags,
    IUnitOfWork unitOfWork) : ITagsService
{
    public async Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = await tags.GetAllAsync(cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tag = await tags.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.TagNotFound);

        tags.Remove(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static TagDto ToDto(Tag tag) => new()
    {
        Id = tag.Id,
        Name = tag.Name,
        CreatedAt = tag.CreatedAt,
    };
}
