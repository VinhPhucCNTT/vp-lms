using Backend.Core.Common;
using Backend.Core.Entities.Users;
using Backend.Core.Types;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

using Isopoh.Cryptography.Argon2;

namespace Backend.Services.Users;

public class UserService(
    IDbContextFactory<AppDbContext> dbFactory
)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<UserDetailResponse?> GetUserByIdAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => new UserDetailResponse(
                u.Id,
                u.Username,
                u.Email,
                u.Fullname,
                u.AvatarUrl,
                u.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<UserStatResponse?> GetUserStatAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return new UserStatResponse(
            await db.Courses
                .AsNoTracking()
                .Where(c => c.CreatorId == userId)
                .CountAsync(),

            await db.Enrollments
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .CountAsync()
        );
    }

    public async Task<QueryResponse<UserResponse>> QueryUsersAsync(UserRequest query)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var users = db.Users.AsNoTracking().Where(u => u.IsActive);

        if (!string.IsNullOrEmpty(query.Username))
            users = users.Where(u => u.Username.Contains(query.Username, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.Email))
            users = users.Where(u => u.Email.Contains(query.Email, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.Fullname))
            users = users.Where(u => u.Fullname.Contains(query.Fullname, StringComparison.OrdinalIgnoreCase));

        var list = await users
            .OrderBy(u => u.Id)
            .Select(u => new UserResponse(
                u.Id,
                u.Username,
                u.AvatarUrl))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new QueryResponse<UserResponse>(
            query.PageNumber,
            query.PageSize,
            await users.CountAsync(),
            list);
    }

    public async Task<bool> AddUserAsync(UserSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var hashedPassword = Argon2.Hash(dto.Password);
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            Fullname = dto.Fullname,
            AvatarUrl = dto.AvatarUrl,
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(long userId, UserSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.Where(u => u.IsActive).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        var hashedPassword = Argon2.Hash(dto.Password);
        user.Username = dto.Username;
        user.Email = dto.Email;
        user.PasswordHash = hashedPassword;
        user.Fullname = dto.Fullname;
        user.AvatarUrl = dto.AvatarUrl;
        user.IsActive = true;

        db.Users.Update(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        AnonymizeUser(user);
        db.Users.Update(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserActiveAsync(long userId) {
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        return user != null && user.IsActive;
    }

    public async Task<bool> DeactivateUserAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.Where(u => u.IsActive).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.IsActive = false;
        db.Users.Update(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateUserAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.Where(u => !u.IsActive).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.IsActive = true;
        db.Users.Update(user);
        await db.SaveChangesAsync();
        return true;
    }

    static private void AnonymizeUser(User user)
    {
        var anonymizeId = Guid.NewGuid().ToString("N");

        user.Username = $"deleted-{anonymizeId}";
        user.Email = $"deleted-{anonymizeId}@removed.com";
        user.PasswordHash = "";
        user.Fullname = "[[DELETED USER]]";
        user.IsActive = false;
        user.AvatarUrl = null;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
    }
}
