using JudgeWorker;

var builder = Host.CreateApplicationBuilder(args);
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

var limit = appSettings!.WorkerCount;
for (int i = 0; i < limit; i++)
    builder.Services.AddHostedService<WorkerMain>();

var host = builder.Build();
host.Run();
