using Backend.Core.Common;
using Backend.Services.Modules.Types;

namespace Backend.Services.Modules.Services;

public interface IModuleService
{
    Task<ModuleResponse?> GetModuleByIdAsync(Guid moduleId);

    Task<QueryResponse<ModuleResponse>?> QueryModulesAsync(Guid courseId, ModuleRequest query);
}
