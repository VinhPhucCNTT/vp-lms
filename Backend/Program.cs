using Backend.Data;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FluentValidation;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Backend.Endpoints;
using Sqids;
using Backend.Services.Common;
using Backend.Services.Auth.Services;

var builder = WebApplication.CreateBuilder(args);

// Automatically register validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "allowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.AddOpenApi();

builder.Services.AddAuthorization();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

var sqidsSettings = builder.Configuration.GetSection("Sqids").Get<SqidsSettings>();
builder.Services.AddSingleton(provider =>
        new SqidsEncoder<int>(new()
        {
            Alphabet = sqidsSettings?.Alphabet!,
            MinLength = sqidsSettings?.MinLength ?? 0
        })
);

// [[ AUTHENTICATION ]]

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),

        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };

    // DEBUG
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context => Task.CompletedTask
    };
});

// Inject services
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.UseCors("allowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Add endpoints
app.AddAuthEndpoints();

app.Run();
