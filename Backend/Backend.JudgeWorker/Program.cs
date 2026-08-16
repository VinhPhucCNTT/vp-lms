using Backend.JudgeWorker.Configuration;
using Backend.JudgeWorker.Interfaces;
using Backend.JudgeWorker.Messaging;
using Backend.JudgeWorker.Services;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<RabbitMqOptions>(
        builder.Configuration.GetSection(
            RabbitMqOptions.SectionName));

// Database
//
// Replace this with your existing infrastructure
// registration.
builder.Services.AddInfrastructure(
    builder.Configuration);

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

// Database adapter
builder.Services.AddScoped<
    ISubmissionStore,
    EfSubmissionStore>();

// RabbitMQ consumer
builder.Services.AddHostedService<
    RabbitMqConsumer>();

var host = builder.Build();

await host.RunAsync();
