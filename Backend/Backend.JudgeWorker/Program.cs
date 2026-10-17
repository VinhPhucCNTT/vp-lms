using Backend.JudgeWorker.Configuration;
using Backend.JudgeWorker.Services;
using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<RabbitMqOptions>(
        builder.Configuration.GetSection("RabbitMq"));

// Database
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

// Application services
builder.Services.AddScoped<
    ISubmissionProcessor,
    SubmissionProcessor>();

builder.Services.AddScoped<
    IJudgeService,
    JudgeService>();

builder.Services.AddSingleton<
    IDockerRunner,
    DockerRunner>();

// RabbitMQ consumer
builder.Services.AddHostedService<
    RabbitMqConsumer>();

var host = builder.Build();

await host.RunAsync();
