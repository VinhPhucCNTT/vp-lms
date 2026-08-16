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
using Backend.Api.Services.Common;
using Backend.Api.Services.Auth;
using Backend.Api.Services.Courses;
using Backend.Api.Services.Users;
using Backend.Api.Core.Automapper;
using Backend.Api.Core.Helpers;
using System.IdentityModel.Tokens.Jwt;
using Backend.Api.Services.Content;
using Backend.Api.Core.Authorization;
using Backend.Persistence.Entities.Users;
using Backend.Api.Services.Assessments.Graders;
using Backend.Api.Services.Assessments.Validators;
using Backend.Api.Services.Assessments;
using Backend.Api.Endpoints.Course;
using Backend.Api.Endpoints.Assessment;
using Backend.Api.Services.Development;

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

var sqidsSettings = builder.Configuration.GetSection("Sqids").Get<Backend.Api.Core.Common.SqidsOptions>();
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
        typeof(UserProfile));

// Inject services
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<AuthenticationService>();

builder.Services.AddScoped<CourseAuthorization>();

builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EnrollmentService>();

builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<ModuleService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<LessonService>();

/////
builder.Services.AddScoped<MultipleChoiceGrader>();
builder.Services.AddScoped<MultipleSelectGrader>();
builder.Services.AddScoped<TrueFalseGrader>();
builder.Services.AddScoped<IQuestionGrader, TrueFalseGrader>();
builder.Services.AddScoped<ShortAnswerGrader>();
builder.Services.AddScoped<DragAndDropGrader>();
builder.Services.AddScoped<CodingGrader>();
builder.Services.AddScoped<QuestionContentValidator>();
builder.Services.AddScoped<IQuestionContentValidator>(sp =>
    sp.GetRequiredService<QuestionContentValidator>());
builder.Services.AddScoped<IQuestionTypeValidator, ChoiceQuestionValidator>();

builder.Services.AddScoped<QuestionSelectionService>();

builder.Services.AddScoped<AssessmentService>();
builder.Services.AddScoped<AssessmentQuestionService>();
builder.Services.AddScoped<AssessmentAttemptService>();
builder.Services.AddScoped<QuestionBankService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<AssessmentGradingService>();
/////

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
app.AddAssessmentEndpoints();
app.AddUserEndpoints();

app.Run();
