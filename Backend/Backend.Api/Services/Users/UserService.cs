using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Api.Services.Content;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Users;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Users;

public interface IUserService
{
    public Task<UserDetailResponse?> GetUserByIdAsync(long userId, CancellationToken ct = default);

    public Task<UserStatResponse?> GetUserStatAsync(long userId, CancellationToken ct = default);

    public Task<PaginatedResponse<UserResponse>> QueryUsersAsync(UserQueryRequest query, CancellationToken ct = default);

    public Task<UserSetResponse> CreateUserAsync(UserCreateRequest dto, CancellationToken ct = default);

    public Task<Result<UserPatchResponse>> UpdateUserAsync(long userId, UserPatchRequest dto, CancellationToken ct = default);

    public Task<bool> DeleteUserAsync(long userId, CancellationToken ct = default);

    public Task<bool> IsUserActiveAsync(long userId, CancellationToken ct = default);

    public Task<bool> DeactivateUserAsync(long userId, CancellationToken ct = default);

    public Task<bool> ActivateUserAsync(long userId, CancellationToken ct = default);
}

public class UserService(
    IFileService fileService,
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    ILogger<UserService> logger) : IUserService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;
    private readonly IFileService _fileService = fileService;
    private readonly ILogger<UserService> _logger = logger;

    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<UserDetailResponse?> GetUserByIdAsync(long userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => _mapper.Map<UserDetailResponse>(u))
            .FirstOrDefaultAsync(cancellationToken: ct);
    }

    public async Task<UserStatResponse?> GetUserStatAsync(long userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return new UserStatResponse(
            await db.Courses
                .AsNoTracking()
                .Where(c => c.InstructorId == userId)
                .CountAsync(cancellationToken: ct),

            await db.Enrollments
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .CountAsync(cancellationToken: ct)
        );
    }

    public async Task<PaginatedResponse<UserResponse>> QueryUsersAsync(UserQueryRequest query, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var users = db.Users.AsNoTracking().Where(u => u.IsActive);

        if (!string.IsNullOrEmpty(query.Email))
            users = users.Where(u => u.Email.Contains(query.Email, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.FirstName))
            users = users.Where(u => u.FirstName.Contains(query.FirstName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.LastName))
            users = users.Where(u => u.LastName.Contains(query.LastName, StringComparison.OrdinalIgnoreCase));

        if (query.Role is not null)
            users = users.Where(u => u.Role == query.Role);

        var list = await users
            .OrderBy(u => u.Id)
            .Select(u => _mapper.Map<UserResponse>(u))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken: ct);

        return new PaginatedResponse<UserResponse>(
            query.PageNumber,
            query.PageSize,
            await users.CountAsync(cancellationToken: ct),
            list);
    }

    public async Task<UserSetResponse> CreateUserAsync(UserCreateRequest dto, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var hashedPassword = Argon2.Hash(dto.Password);
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = hashedPassword,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return _mapper.Map<UserSetResponse>(user);
    }

    public async Task<Result<UserPatchResponse>> UpdateUserAsync(long userId, UserPatchRequest dto, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var user = await db.Users.Where(u => u.IsActive).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);
        if (user == null)
            return Result<UserPatchResponse>.Failure(UserErrors.NotFound);

        if (dto.Password is not null)
            user.PasswordHash = Argon2.Hash(dto.Password);

        if (dto.Email is not null)
        user.Email = dto.Email;

        if (dto.FirstName is not null)
        user.FirstName = dto.FirstName;

        if (dto.LastName is not null)
        user.LastName = dto.LastName;

        if (dto.AvatarFile is not null)
        {
            var result = await SaveAvatarAsync(dto.AvatarFile, ct);
            if (!result.IsSuccess)
                return Result<UserPatchResponse>.Failure(UserErrors.Upload);

            user.AvatarFileId = result.Value;
        }

        // user.IsActive = true;

        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
        return Result.Success(_mapper.Map<UserPatchResponse>(user));
    }

    public async Task<bool> DeleteUserAsync(long userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);
        if (user == null) return false;

        AnonymizeUser(user);
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IsUserActiveAsync(long userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);
        return user?.IsActive == true;
    }

    public async Task<bool> DeactivateUserAsync(long userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var user = await db.Users.Where(u => u.IsActive).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);
        if (user == null) return false;

        user.IsActive = false;
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ActivateUserAsync(long userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var user = await db.Users.Where(u => !u.IsActive).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);
        if (user == null) return false;

        user.IsActive = true;
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
        return true;
    }

    static private void AnonymizeUser(User user)
    {
        var anonymizeId = Guid.NewGuid().ToString("N");

        user.Email = $"deleted-{anonymizeId}@removed.com";
        user.PasswordHash = "";
        user.FirstName = "[[DELETED USER]]";
        user.LastName = "";
        user.IsActive = false;
        user.AvatarFileId = null;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
    }

    private async Task<Result<long>> SaveAvatarAsync(IFormFile avatarFile, CancellationToken ct = default)
    {
        await using var stream = avatarFile.OpenReadStream();
        var result = await _fileService.UploadAsync(
            stream,
            avatarFile.FileName,
            avatarFile.ContentType,
            _currentUserService.UserId,
            FileCategory.Avatar,
            ct);
        if (!result.IsSuccess)
        {
            _logger.LogError("UploadAsync failed.");
            return Result<long>.Failure(UserErrors.Upload);
        }

        return Result.Success(result.Value);
    }
}
