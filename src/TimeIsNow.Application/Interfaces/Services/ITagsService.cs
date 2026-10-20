using TimeIsNow.Application.Features.Tags;

namespace TimeIsNow.Application.Interfaces.Services;

public interface ITagsService
{
    Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
