using TimeIsNow.Application.Features.Modules;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IModulesService
{
    Task<ModuleSetDto> GetAllScopeModulesAsync(CancellationToken cancellationToken = default);
    Task<ModuleSetDto> UpdateAllScopeModulesAsync(UpdateModulesRequest request, CancellationToken cancellationToken = default);
    Task<ModuleSetDto> UpdateCategoryModulesAsync(int categoryId, UpdateModulesRequest request, CancellationToken cancellationToken = default);
}
