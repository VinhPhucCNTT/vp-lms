using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Judge;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Judge;

public interface ICompilerService
{
    public Task<CompilerResponse?> GetAsync(long compilerId, CancellationToken ct = default);

    public Task<PaginatedResponse<CompilerResponse>> QueryAsync(CompilerQueryRequest query, CancellationToken ct = default);

    public Task<Result<CompilerResponse>> CreateAsync(CompilerRequest request, CancellationToken ct = default);

    public Task<Result<CompilerResponse>> UpdateAsync(long compilerId, CompilerRequest request, CancellationToken ct = default);

    public Task<Result<bool>> DeleteAsync(long compilerId, CancellationToken ct = default);
}

public class CompilerService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper,
    ILogger<CompilerService> logger) : ICompilerService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CompilerService> _logger = logger;

    public async Task<CompilerResponse?> GetAsync(long compilerId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Compilers
            .AsNoTracking()
            .Where(c => c.Id == compilerId)
            .Select(c => _mapper.Map<CompilerResponse>(c))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedResponse<CompilerResponse>> QueryAsync(CompilerQueryRequest query, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var compilers = db.Compilers.AsNoTracking();

        if (!String.IsNullOrEmpty(query.Name))
            compilers = compilers.Where(c => c.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase));

        if (!String.IsNullOrEmpty(query.SourceFileName))
            compilers = compilers.Where(c => c.SourceFileName.Contains(query.SourceFileName, StringComparison.OrdinalIgnoreCase));

        if (!String.IsNullOrEmpty(query.ApplicationName))
            compilers = compilers.Where(c => c.ApplicationName.Contains(query.ApplicationName, StringComparison.OrdinalIgnoreCase));

        if (!String.IsNullOrEmpty(query.Version))
            compilers = compilers.Where(c => c.Version.Contains(query.Version, StringComparison.OrdinalIgnoreCase));

        if (query.Language is not null)
            compilers = compilers.Where(c => c.Language == query.Language);

        var list = await compilers
            .OrderBy(c => c.Id)
            .Select(c => _mapper.Map<CompilerResponse>(c))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PaginatedResponse<CompilerResponse>(
            query.PageNumber,
            query.PageSize,
            await compilers.CountAsync(ct),
            list);
    }

    public async Task<Result<CompilerResponse>> CreateAsync(CompilerRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        Compiler? compiler;
        try
        {
            compiler = _mapper.Map<Compiler>(request);
            if (compiler is null)
                throw new Exception("Cannot map request to db entity.");

            db.Compilers.Add(compiler);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when adding new compiler.");
            return Result<CompilerResponse>.Failure(CompilerErrors.Internal);
        }

        return Result.Success(_mapper.Map<CompilerResponse>(compiler));
    }

    public async Task<Result<CompilerResponse>> UpdateAsync(long compilerId, CompilerRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        Compiler? compiler;
        try
        {
            compiler = await db.Compilers.FirstOrDefaultAsync(c => c.Id == compilerId);
            if (compiler is null)
            {
                _logger.LogError("Compiler not found.");
                return Result<CompilerResponse>.Failure(CompilerErrors.NotFound);
            }

            compiler.Name = request.Name;
            compiler.Language = request.Language;
            compiler.SourceFileName = request.SourceFileName;
            compiler.ApplicationName = request.ApplicationName;
            compiler.Version = request.Version;

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when updating compiler.");
            return Result<CompilerResponse>.Failure(CompilerErrors.Internal);
        }

        return Result.Success(_mapper.Map<CompilerResponse>(compiler));
    }

    public async Task<Result<bool>> DeleteAsync(long compilerId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        try
        {
            int count = await db.Compilers
                .Where(c => c.Id == compilerId)
                .ExecuteDeleteAsync(ct);

            if (count < 1)
            {
                _logger.LogError("Compiler not found.");
                return Result<bool>.Failure(CompilerErrors.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when deleting compiler.");
            return Result<bool>.Failure(CompilerErrors.Internal);
        }

        return Result.Success(true);
    }
}
