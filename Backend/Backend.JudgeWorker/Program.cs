using Backend.JudgeWorker;
using Backend.JudgeWorker.Configuration;
using Backend.JudgeWorker.Data;
using Backend.JudgeWorker.Interfaces;
using Backend.JudgeWorker.Languages;
using Backend.JudgeWorker.Messaging;
using Backend.JudgeWorker.Services;
using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<RabbitMqOptions>(
        builder.Configuration.GetSection(
            RabbitMqOptions.SectionName));

// Database
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

// Application services
builder.Services.AddSingleton<
    ILanguageDefinitionProvider,
    LanguageDefinitionProvider>();

builder.Services.AddScoped<
    ISubmissionProcessor,
    SubmissionProcessor>();

builder.Services.AddScoped<
    IJudgeService,
    JudgeService>();

builder.Services.AddSingleton<
    IDockerRunner,
    DockerRunner>();

// Database adapter
builder.Services.AddScoped<
    ISubmissionStore,
    EfSubmissionStore>();

// RabbitMQ consumer
builder.Services.AddHostedService<
    RabbitMqConsumer>();

var host = builder.Build();

await host.RunAsync();
