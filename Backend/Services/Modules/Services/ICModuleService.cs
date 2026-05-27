using Backend.Core.Common;
using Backend.Services.Modules.Types;

namespace Backend.Services.Modules.Services;

public interface ICModuleService
{
    Task<MResult<CModuleResponse, CModuleGetError>> GetModuleByIdAsync(Guid userId, Guid moduleId);

    Task<QueryResponse<CModuleResponse>> QueryModulesAsync(Guid userId, Guid courseId, CModuleRequest query);

    Task<MResult<CModuleSetResponse, CModuleSetError>> AddModuleAsync(Guid userId, Guid courseId, CModuleSetRequest dto);

    Task<MResult<CModuleSetResponse, CModuleSetError>> UpdateModuleAsync(Guid userId, Guid courseId, Guid moduleId, CModuleSetRequest dto);

    Task<CModuleDeleteStatus> DeleteModuleAsync(Guid userId, Guid courseId, Guid moduleId);
}
