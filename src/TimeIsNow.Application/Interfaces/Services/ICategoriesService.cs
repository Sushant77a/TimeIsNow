using TimeIsNow.Application.Features.Categories;

namespace TimeIsNow.Application.Interfaces.Services;

public interface ICategoriesService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
