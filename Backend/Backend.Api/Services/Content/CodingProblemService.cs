using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Persistence.Common;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Judge;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Content;

public interface ICodingProblemService
{
    public Task<Result<CodingProblemResponse>>
        CreateAsync(long moduleId, CodingProblemRequest request, CancellationToken ct);

    public Task<Result<CodingProblemResponse>>
        GetByResourceAsync(long resourceId, CancellationToken ct);

    public Task<Result<ProblemTestCaseResponse>>
        AddTestCaseAsync(long resourceId, ProblemTestCaseRequest request, CancellationToken ct);

    public Task<Result<IReadOnlyList<ProblemTestCaseResponse>>>
        GetTestCasesAsync(
            long resourceId,
            CancellationToken ct);

    public Task<Result>
        GetLanguageProfileAsync(
            long resourceId,
            CancellationToken ct);

    public Task<Result>
        SetLanguageProfileAsync(
            long resourceId,
            LanguageProfileResponse[] request,
            CancellationToken ct);
}

public sealed class CodingProblemService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUser,
    IMapper mapper,
    ILogger<CodingProblemService> logger)
{
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CodingProblemService> _logger = logger;

    public async Task<Result<CodingProblemResponse>> CreateAsync(
        long moduleId,
        CodingProblemRequest request,
        CancellationToken ct = default)
    {
        // if (!TryParseLanguage(
        //         request.Info.Language,
        //         out _))
        // {
        //     return Result<CodingProblemResponse>.Failure(
        //         ErrorType.Validation,
        //         $"Unsupported programming language '{request.Info.Language}'.");
        // }

        await using var db =
            await dbFactory.CreateDbContextAsync(ct);

        var module = await db.CourseModules
            .Include(x => x.Course)
            .SingleOrDefaultAsync(
                x => x.Id == moduleId,
                ct);

        if (module is null)
            return Result<CodingProblemResponse>.Failure(
                ErrorType.NotFound,
                "Module not found.");

        if (module.Course.CreatorId != currentUser.UserId)
            return Result<CodingProblemResponse>.Failure(
                ErrorType.Forbidden,
                "You do not own this course.");

        await using var transaction =
            await db.Database.BeginTransactionAsync(ct);

        try
        {
            var resource = new CourseResource
            {
                ModuleId = moduleId,
                Type = ResourceType.Problem,
                Title = request.ResourceInfo.Title,
                OrderIndex = request.ResourceInfo.OrderIndex,
                IsPublished = request.ResourceInfo.IsPublished
            };

            db.CourseResources.Add(resource);

            // var problem = new CodingProblem
            // {
            //     Resource = resource,
            //     ProblemStatementMarkdown =
            //         request.Info.ProblemStatementMarkdown,
            //     ConstraintsMarkdown =
            //         request.Info.ConstraintsMarkdown,
            //     FunctionSignature =
            //         request.Info.FunctionSignature,
            //     TimeLimitMs =
            //         request.Info.TimeLimitMs,
            //     MemoryLimitMb =
            //         request.Info.MemoryLimitMb,
            //     IsPractice =
            //         request.Info.IsPractice,
            //     Difficulty =
            //         request.Info.Difficulty
            // };
            var problem = _mapper.Map<CodingProblem>(request);
            problem.Resource = resource;

            db.CodingProblems.Add(problem);

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            var response = new CodingProblemResponse(
                _mapper.Map<ResourceDetailResponse>(resource),
                _mapper.Map<CodingProblemInfo>(problem));

            return Result<CodingProblemResponse>.Success(
                response);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);

            return Result<CodingProblemResponse>.Failure(
                new Error(
                    "system_error",
                    "Failed to create coding problem."));
        }
    }

    public async Task<Result<CodingProblemResponse>> GetByResourceIdAsync(
        long resourceId,
        CancellationToken ct)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(ct);

        var problem = await db.CodingProblems
            .Include(x => x.Resource)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ResourceId == resourceId &&
                     !x.IsDeleted,
                ct);

        if (problem is null)
        {
            return Result<CodingProblemResponse>.Failure(
                ErrorType.NotFound,
                "Coding problem not found.");
        }

        var isOwner = await db.CourseResources
            .Where(x => x.Id == resourceId)
            .Where(x =>
                x.Module.Course.CreatorId ==
                currentUser.UserId)
            .AnyAsync(ct);

        if (!problem.Resource.IsPublished && !isOwner)
        {
            return Result<CodingProblemResponse>.Failure(
                ErrorType.NotFound,
                "Coding problem not found.");
        }

        return Result<CodingProblemResponse>.Success(
            new CodingProblemResponse(
                _mapper.Map<ResourceDetailResponse>(
                    problem.Resource),
                _mapper.Map<CodingProblemInfo>(
                    problem)));
    }

    public async Task<Result<ProblemTestCaseResponse>> AddTestCaseAsync(
        long resourceId,
        ProblemTestCaseRequest request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Result<ProblemTestCaseResponse>.Failure(
                ErrorType.Forbidden,
                "Authentication is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ExpectedOutput))
        {
            return Result<ProblemTestCaseResponse>.Failure(
                ErrorType.Validation,
                "Expected output is required.");
        }

        await using var db =
            await dbFactory.CreateDbContextAsync(ct);

        var problem = await db.CodingProblems
            .Include(x => x.Resource)
                .ThenInclude(x => x.Module)
                    .ThenInclude(x => x.Course)
            .SingleOrDefaultAsync(
                x => x.ResourceId == resourceId &&
                     !x.IsDeleted,
                ct);

        if (problem is null)
        {
            return Result<ProblemTestCaseResponse>.Failure(
                ErrorType.NotFound,
                "Coding problem not found.");
        }

        if (problem.Resource.Module.Course.CreatorId !=
            currentUser.UserId)
        {
            return Result<ProblemTestCaseResponse>.Failure(
                ErrorType.Forbidden,
                "You do not own this course.");
        }

        var nextOrder =
            (await db.ProblemTestCases
                .Where(x => x.ProblemId == problem.Id)
                .MaxAsync(
                    x => (int?)x.OrderIndex,
                    ct) ?? 0) + 1;

        var testCase = new ProblemTestCase
        {
            ProblemId = problem.Id,
            OrderIndex = nextOrder,
            Input = request.Input,
            ExpectedOutput = request.ExpectedOutput
        };

        db.ProblemTestCases.Add(testCase);

        await db.SaveChangesAsync(ct);

        return Result<ProblemTestCaseResponse>.Success(_mapper.Map<ProblemTestCaseResponse>(testCase));
    }

    public async Task<Result<IReadOnlyList<ProblemTestCaseResponse>>>
        GetTestCasesAsync(
            long resourceId,
            CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Result<IReadOnlyList<ProblemTestCaseResponse>>.Failure(
                ErrorType.Forbidden,
                "Authentication is required.");
        }

        await using var db =
            await dbFactory.CreateDbContextAsync(ct);

        var problem = await db.CodingProblems
            .Include(x => x.Resource)
            .ThenInclude(x => x.Module)
            .ThenInclude(x => x.Course)
            .SingleOrDefaultAsync(
                x => x.ResourceId == resourceId &&
                     !x.IsDeleted,
                ct);

        if (problem is null)
        {
            return Result<IReadOnlyList<ProblemTestCaseResponse>>.Failure(
                ErrorType.NotFound,
                "Coding problem not found.");
        }

        var isOwner =
            problem.Resource.Module.Course.CreatorId ==
            currentUser.UserId;

        if (!problem.Resource.IsPublished && !isOwner)
        {
            return Result<IReadOnlyList<ProblemTestCaseResponse>>.Failure(
                ErrorType.NotFound,
                "Coding problem not found.");
        }

        var testCases = await db.ProblemTestCases
            .AsNoTracking()
            .Where(x => x.ProblemId == problem.Id)
            .OrderBy(x => x.OrderIndex)
            .Select(x => _mapper.Map<ProblemTestCaseResponse>(x))
            .ToListAsync(ct);
        return Result<IReadOnlyList<ProblemTestCaseResponse>>.Success(
            testCases);
    }

    public async Task<List<LanguageProfileResponse>?> GetLanguageProfileAsync(
            long resourceId,
            CancellationToken ct = default)
    {
        using var db = await dbFactory.CreateDbContextAsync(ct);
        var problem = await db.CodingProblems
            .AsNoTracking()
            .Where(p => p.ResourceId == resourceId)
            .FirstOrDefaultAsync(ct);
        if (problem is null)
            return null;

        return await GetProfileDto(db, problem.LanguageProfile, ct);
    }

    public async Task<List<LanguageProfileResponse>?> SetLanguageProfileAsync(
            long resourceId,
            LanguageProfileRequest[] request,
            CancellationToken ct)
    {
        using var db = await dbFactory.CreateDbContextAsync(ct);
        var problem = await db.CodingProblems
            .AsNoTracking()
            .Where(p => p.ResourceId == resourceId)
            .FirstOrDefaultAsync(ct);
        if (problem is null)
            return null;

        LanguageProfile[] result = [];
        foreach (var r in request)
        {
            var compiler = await db.Compilers.AsNoTracking().SingleOrDefaultAsync(c => c.Name == r.CompilerName);
            if (compiler is null)
            {
                _logger.LogWarning("No compiler exists with name {CompilerName}.", r.CompilerName);
                continue;
            }

            result.Append(new LanguageProfile
            {
                CompilerId = compiler.Id,
                Name = r.Name,
                CompilerCmd = r.CompilerCmd,
                RunCmd = r.RunCmd
            });
        }

        problem.LanguageProfile = result;
        await db.SaveChangesAsync(ct);

        return await GetProfileDto(db, result, ct);
    }

    private async Task<List<LanguageProfileResponse>> GetProfileDto(AppDbContext db, LanguageProfile[] languageProfiles, CancellationToken ct = default)
    {
        List<LanguageProfileResponse> result = [];
        foreach (var profile in languageProfiles)
        {
            var compiler = await db.Compilers.AsNoTracking().SingleOrDefaultAsync(c => c.Id == profile.CompilerId);
            if (compiler is null)
            {
                _logger.LogWarning("Compiler not defined for profile {ProfileName}.", profile.Name);
                continue;
            }

            result.Append(new LanguageProfileResponse(
                CompilerInfo: _mapper.Map<CompilerDto>(compiler),
                Name: profile.Name,
                CompilerCmd: profile.CompilerCmd,
                RunCmd: profile.RunCmd
            ));
        }

        return result;
    }

    private static bool TryParseLanguage(
        string value,
        out ProgrammingLanguage language)
    {
        return Enum.TryParse(
            value,
            ignoreCase: true,
            out language);
    }
}
