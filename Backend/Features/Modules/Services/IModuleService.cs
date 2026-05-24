using Backend.Common;
using Backend.Features.Modules.Types;

namespace Backend.Features.Modules.Services;

public interface IModuleService
{
    Task<ModuleResponse?> GetModuleByIdAsync(Guid moduleId);

    Task<QueryResponse<ModuleResponse>?> QueryModulesAsync(Guid courseId, ModuleRequest query);
}
