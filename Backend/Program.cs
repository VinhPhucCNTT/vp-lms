using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FluentValidation;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Sqids;

using Backend.Data;
using Backend.Endpoints;
using Backend.Core.Common;
using Backend.Services.Common;
using Backend.Services.Auth;
using Backend.Services.Courses;
using Backend.Services.Users;
using Backend.Core.Automapper;

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
    new SqidsEncoder<long>(new()
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

builder.Services.AddHttpContextAccessor();

// Automapper
builder.Services.AddTransient<SqidConverter>();
builder.Services.AddAutoMapper(cfg => { }, typeof(CourseProfile));

// Inject services
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<ResourceService>();

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
