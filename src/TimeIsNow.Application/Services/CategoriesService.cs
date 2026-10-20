using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Categories;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class CategoriesService(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork) : ICategoriesService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        var results = await categories.GetAllAsync(includeArchived, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ToDto(await FindRequiredAsync(id, cancellationToken));

    public async Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsArchived = request.IsArchived,
            CreatedAt = now,
            UpdatedAt = now,
        };

        categories.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await FindRequiredAsync(id, cancellationToken);
        category.Name = request.Name;
        category.Description = request.Description;
        category.Color = request.Color;
        category.Icon = request.Icon;
        category.SortOrder = request.SortOrder;
        category.IsArchived = request.IsArchived;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await FindRequiredAsync(id, cancellationToken);

        if (await categories.HasActiveItemsAsync(id, cancellationToken))
            throw new Conflict409Exception(ApiMessages.CategoryHasItems);

        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await categories.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.CategoryNotFound);

    private static CategoryDto ToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Color = category.Color,
        Icon = category.Icon,
        SortOrder = category.SortOrder,
        IsArchived = category.IsArchived,
        Modules = [.. category.Modules
            .OrderBy(m => m.SortOrder)
            .Select(m => Common.Constants.ModuleKeys.ToName(m.ModuleKey))],
        CreatedAt = category.CreatedAt,
        UpdatedAt = category.UpdatedAt,
    };
}
