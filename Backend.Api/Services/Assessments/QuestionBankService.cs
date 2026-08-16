using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using AutoMapper;
using Backend.Api.Core.Entities.Assessments;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments;

public sealed class QuestionBankService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<QuestionBankInfo>> GetByIdAsync(
        long bankId,
        CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var bank = await db.QuestionBanks
            .AsNoTracking()
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == bankId, ct);

        if (bank is null)
            return Result<QuestionBankInfo>.Failure(
                QuestionErrors.NotFound);

        // if (!await CanViewAsync(db, bank, instructorId, ct))
        // {
        //     return Result<QuestionBankInfo>.Failure(
        //         QuestionErrors.Forbidden);
        // }

        return Result<QuestionBankInfo>.Success(_mapper.Map<QuestionBankInfo>(bank));
    }

    public async Task<Result<QuestionBankInfo>> CreateAsync(
          QuestionBankInfo request,
          CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var bank = new QuestionBank
        {
            OwnerId = instructorId,
            Name = request.Name,
            Description = request.Description
        };

        db.QuestionBanks.Add(bank);
        await db.SaveChangesAsync(ct);
        return Result<QuestionBankInfo>.Success(_mapper.Map<QuestionBankInfo>(bank));
    }

    public async Task<Result> UpdateAsync(
           long bankId,
           QuestionBankInfo request,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var bank = await db.QuestionBanks
            .FirstOrDefaultAsync(x => x.Id == bankId, ct);

        if (bank is null)
            return Result.Failure(QuestionErrors.NotFound);

        if (bank.OwnerId != instructorId)
            return Result.Failure(QuestionErrors.Forbidden);

        bank.Name = request.Name;
        bank.Description = request.Description;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
           long bankId,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var bank = await db.QuestionBanks
            .FirstOrDefaultAsync(x => x.Id == bankId, ct);

        if (bank is null)
            return Result.Failure(QuestionErrors.NotFound);

        if (bank.OwnerId != instructorId)
            return Result.Failure(QuestionErrors.Forbidden);

        bank.IsDeleted = true;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    // private static async Task<bool> CanViewAsync(
    //      AppDbContext db,
    //      QuestionBank bank,
    //      long instructorId,
    //      CancellationToken ct)
    // {
    //     if (bank.OwnerId == instructorId)
    //         return true;
    //
    //     return await db.QuestionBankShares
    //         .AnyAsync(
    //             x => x.QuestionBankId == bank.Id &&
    //                  x.InstructorId == instructorId,
    //             ct);
    // }
}
