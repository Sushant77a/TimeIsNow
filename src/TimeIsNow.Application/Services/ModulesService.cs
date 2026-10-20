using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Modules;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Security;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class ModulesService(
    ICategoryModuleRepository moduleRepository,
    ICategoryRepository categories,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IModulesService
{
    public async Task<ModuleSetDto> GetAllScopeModulesAsync(CancellationToken cancellationToken = default)
    {
        var existing = await moduleRepository.GetGlobalModulesAsync(cancellationToken);
        return new ModuleSetDto { Modules = [.. ModuleKeys.ToNames(existing.Select(m => m.ModuleKey))] };
    }

    public async Task<ModuleSetDto> UpdateAllScopeModulesAsync(UpdateModulesRequest request, CancellationToken cancellationToken = default)
    {
        var keys = ModuleKeys.ParseNames(request.Modules);

        var existing = await moduleRepository.GetGlobalModulesAsync(cancellationToken);
        moduleRepository.RemoveGlobalRange(existing);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = currentUser.UserId;
        var now = DateTime.UtcNow;
        var order = 0;
        foreach (var key in keys)
        {
            moduleRepository.AddGlobal(new GlobalModule
            {
                UserId = userId,
                ModuleKey = key,
                SortOrder = order++,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ModuleSetDto { Modules = [.. ModuleKeys.ToNames(keys)] };
    }

    public async Task<ModuleSetDto> UpdateCategoryModulesAsync(int categoryId, UpdateModulesRequest request, CancellationToken cancellationToken = default)
    {
        var category = await categories.FindByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.CategoryNotFound);

        var keys = ModuleKeys.ParseNames(request.Modules);

        var existing = await moduleRepository.GetModulesByCategoryIdAsync(categoryId, cancellationToken);
        moduleRepository.RemoveCategoryRange(existing);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = currentUser.UserId;
        var now = DateTime.UtcNow;
        var order = 0;
        foreach (var key in keys)
        {
            moduleRepository.AddCategory(new CategoryModule
            {
                UserId = userId,
                CategoryId = category.Id,
                ModuleKey = key,
                SortOrder = order++,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ModuleSetDto { Modules = [.. ModuleKeys.ToNames(keys)] };
    }
}
