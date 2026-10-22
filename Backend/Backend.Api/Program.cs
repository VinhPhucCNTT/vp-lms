using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FluentValidation;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Sqids;

using Backend.Persistence.Data;
using Backend.Api.Endpoints;
using Backend.Api.Core.Automapper;
using Backend.Api.Core.Helpers;
using System.IdentityModel.Tokens.Jwt;
using Backend.Persistence.Entities.Users;
using Backend.Api.Services.Assessments.Graders;
using Backend.Api.Endpoints.Course;
using Backend.Api.Endpoints.Assessment;
using Backend.Api.Endpoints.Assignment;
using Backend.Api.Services.Development;
using Backend.Api.Core.Common;
using Backend.Api.Services.Common;
using Backend.Api.Services.Assessments.Validators;
using Backend.Api.Core.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// Authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("IsStudent", options => {
        options.RequireAuthenticatedUser();
        options.RequireRole(UserRoles.Student.ToString(), UserRoles.Admin.ToString());
    })
    .AddPolicy("IsInstructor", options => {
        options.RequireAuthenticatedUser();
        options.RequireRole(UserRoles.Instructor.ToString(), UserRoles.Admin.ToString());
    });

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

builder.Services.Configure<Backend.Api.Core.Common.FileOptions>(
    builder.Configuration.GetSection("FileStorage"));

var sqidsSettings = builder.Configuration.GetSection("Sqids").Get<Backend.Api.Core.Common.SqidsOptions>();
builder.Services.AddSingleton(provider =>
    new SqidsEncoder<long>(new()
    {
        Alphabet = sqidsSettings?.Alphabet!,
        MinLength = sqidsSettings?.MinLength ?? 0
    })
);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

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
        NameClaimType = JwtRegisteredClaimNames.Sub
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
builder.Services.AddAutoMapper(cfg => { },
        typeof(CourseProfile),
        typeof(EnrollmentProfile),
        typeof(ModuleProfile),
        typeof(ResourceProfile),
        typeof(UserProfile),
        typeof(AssignmentProfile),
        typeof(FileProfile));

builder.Services.Scan(scan => scan
    .FromAssembliesOf(typeof(Program))
    .AddClasses(classes => classes
        .InNamespaces(
            "Backend.Api.Services.Assessments",
            "Backend.Api.Services.Auth",
            "Backend.Api.Services.Content",
            "Backend.Api.Services.Courses",
            "Backend.Api.Services.Judge",
            "Backend.Api.Services.Submissions",
            "Backend.Api.Services.Users"
        )
    )
    .AsImplementedInterfaces()
    .WithScopedLifetime()

    .AddClasses(classes => classes
        .AssignableTo<IQuestionGrader>()
        .InNamespaces("Backend.Api.Services.Assessments.Graders"))
    .As<IQuestionGrader>()
    .WithScopedLifetime()

    .AddClasses(classes => classes
        .AssignableTo<IQuestionTypeValidator>()
        .InNamespaces("Backend.Api.Services.Assessments.Validators"))
    .As<IQuestionTypeValidator>()
    .WithScopedLifetime()
);

// Manual service registrations
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<IQuestionContentValidator, QuestionContentValidator>();

builder.Services.AddScoped<ICourseAuthorization, CourseAuthorization>();
// builder.Services.AddScoped<IAssessmentAuthorization, AssessmentAuthorization>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors("allowFrontend");

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedAsync(app.Services);

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.AddDevEndpoints();
}

app.UseAuthentication();
app.UseAuthorization();

// Add endpoints
app.AddAuthEndpoints();
app.AddCourseEndpoints();
app.AddEnrollmentEndpoints();
app.AddModuleEndpoints();
app.AddResourceEndpoints();
app.AddLessonEndpoints();
app.AddAssignmentEndpoints();
app.AddFileEndpoints();
app.AddAssessmentEndpoints();
app.AddUserEndpoints();
app.AddCodingProblemEndpoints();
app.AddCodingSubmissionEndpoints();

app.Run();
