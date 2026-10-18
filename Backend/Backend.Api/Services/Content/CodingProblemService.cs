using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
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

    public Task<Result<List<CompilerProfileResponse>>>
        GetLanguageProfilesAsync(
            long resourceId,
            CancellationToken ct);

    public Task<Result<CompilerProfileResponse>>
        SetLanguageProfileAsync(
            long resourceId,
            long compilerId,
            CompilerProfileResponse request,
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
                ResultError.NotFound,
                "Module not found.");

        if (module.Course.CreatorId != currentUser.UserId)
            return Result<CodingProblemResponse>.Failure(
                ResultError.Forbidden,
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

            return Result.Success(response);
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
                ResultError.NotFound,
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
                ResultError.NotFound,
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
                ResultError.Forbidden,
                "Authentication is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ExpectedOutput))
        {
            return Result<ProblemTestCaseResponse>.Failure(
                ResultError.Validation,
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
                ResultError.NotFound,
                "Coding problem not found.");
        }

        if (problem.Resource.Module.Course.CreatorId !=
            currentUser.UserId)
        {
            return Result<ProblemTestCaseResponse>.Failure(
                ResultError.Forbidden,
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
                ResultError.Forbidden,
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
                ResultError.NotFound,
                "Coding problem not found.");
        }

        var isOwner =
            problem.Resource.Module.Course.CreatorId ==
            currentUser.UserId;

        if (!problem.Resource.IsPublished && !isOwner)
        {
            return Result<IReadOnlyList<ProblemTestCaseResponse>>.Failure(
                ResultError.NotFound,
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

    public async Task<Result<List<CompilerProfileResponse>>> GetLanguageProfilesAsync(
            long resourceId,
            CancellationToken ct = default)
    {
        using var db = await dbFactory.CreateDbContextAsync(ct);
        var problem = await db.CodingProblems
            .AsNoTracking()
            .Where(p => p.ResourceId == resourceId)
            .FirstOrDefaultAsync(ct);
        if (problem is null)
            return Result<List<CompilerProfileResponse>>.Failure(ResultError.NotFound, "");

        List<CompilerProfileResponse> result = [];
        foreach (var profile in problem.Profiles)
        {
            result.Append(new CompilerProfileResponse(
                CompilerInfo: _mapper.Map<CompilerDto>(profile.Compiler),
                Name: profile.Name,
                CompileCmd: profile.CompileCmd,
                RunCmd: profile.RunCmd
            ));
        }

        return Result.Success(result);
    }

    public async Task<Result<CompilerProfileResponse>> SetLanguageProfileAsync(
            long resourceId,
            long compilerId,
            CompilerProfileRequest request,
            CancellationToken ct)
    {
        using var db = await dbFactory.CreateDbContextAsync(ct);
        var problem = await db.CodingProblems
            .AsNoTracking()
            .Where(p => p.ResourceId == resourceId)
            .FirstOrDefaultAsync(ct);
        if (problem is null)
            return Result<CompilerProfileResponse>.Failure(ResultError.NotFound, "");

        if (!await db.Compilers.AnyAsync(c => c.Id == compilerId))
            return Result<CompilerProfileResponse>.Failure(ResultError.NotFound, "");

        var profile = problem.Profiles.SingleOrDefault(p => p.CompilerId == compilerId);
        if (profile is null)
            profile = new CompilerProfile
            {
                ProblemId = problem.Id,
                CompilerId = compilerId
            };

        profile.Name = request.Name;
        profile.CompileCmd = request.CompileCmd;
        profile.RunCmd = request.RunCmd;

        await db.SaveChangesAsync(ct);
        return Result.Success(new CompilerProfileResponse(
            CompilerInfo: _mapper.Map<CompilerDto>(profile.Compiler),
            Name: profile.Name,
            CompileCmd: profile.CompileCmd,
            RunCmd: profile.RunCmd
        ));
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
