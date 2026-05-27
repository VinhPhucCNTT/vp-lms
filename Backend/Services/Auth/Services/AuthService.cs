using Backend.Data;
using Backend.Services.Auth.Dtos;
using Backend.Core.Entities.Users;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Isopoh.Cryptography.Argon2;

namespace Backend.Services.Auth.Services;

public class AuthService(
        IConfiguration config,
        IDbContextFactory<AppDbContext> contextFactory
        ) : IAuthService
{
    private readonly IConfiguration _config = config;
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public async Task<LoginResponse?> LoginAsync(LoginRequest dto)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
            return null;

        if (!Argon2.Verify(user.PasswordHash, dto.Password))
            return null;

        return new LoginResponse(user.Email, GenerateToken(user));
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest dto)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        if (!await dbContext.Users.AnyAsync(u => u.Email == dto.Email))
            return null;

        var hashedPassword = Argon2.Hash(dto.Password);
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Fullname = dto.Fullname,
            AvatarUrl = dto.AvatarUrl,
            PasswordHash = hashedPassword
        };

        dbContext.Users.Add(user);
        // TODO: Add error handling
        await dbContext.SaveChangesAsync();

        return new RegisterResponse(user.Username, user.Email);
    }

    private string GenerateToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Username)
        };

        // TODO: Handle admin role
        // claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
