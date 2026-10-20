using TimeIsNow.Application.Features.Notes;

namespace TimeIsNow.Application.Interfaces.Services;

public interface INotesService
{
    Task<IReadOnlyList<NoteDto>> GetListAsync(NoteListQuery query, CancellationToken cancellationToken = default);
    Task<NoteDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<NoteDto> CreateAsync(NoteRequest request, CancellationToken cancellationToken = default);
    Task<NoteDto> UpdateAsync(int id, NoteRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
