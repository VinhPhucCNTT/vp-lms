using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Api.Core.Types;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Users;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sqids;

namespace Backend.Api.Services.Auth;

public interface IAuthenticationService
{
    public Task<LoginResponse?> LoginAsync(LoginRequest dto, CancellationToken ct = default);
    public Task<RegisterResponse?> RegisterAsync(RegisterRequest dto, CancellationToken ct = default);
}

public class AuthenticationService(
    IConfiguration config,
    IDbContextFactory<AppDbContext> contextFactory,
    SqidsEncoder<long> sqidsEncoder) : IAuthenticationService
{
    private readonly IConfiguration _config = config;
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async
        Task<LoginResponse?>
        LoginAsync(
            LoginRequest dto,
            CancellationToken ct = default)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(ct);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
        if (user is null)
            return null;

        if (!Argon2.Verify(user.PasswordHash, dto.Password))
            return null;

        var encodedFileId = user.AvatarFileId is long id
            ? _sqidsEncoder.Encode(id)
            : null;

        return new LoginResponse(
                user.Email,
                GenerateToken(user),
                user.FirstName,
                user.LastName,
                encodedFileId,
                user.IsActive,
                user.Role);
    }

    public async
        Task<RegisterResponse?>
        RegisterAsync(
            RegisterRequest dto,
            CancellationToken ct = default)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync(ct);

        if (await dbContext.Users.AnyAsync(u => u.Email == dto.Email, ct))
            return null;

        var hashedPassword = Argon2.Hash(dto.Password);
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = hashedPassword,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);

        return new RegisterResponse(user.Email, user.FirstName, user.LastName, user.Role);
    }

    private string GenerateToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["DurationInMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
