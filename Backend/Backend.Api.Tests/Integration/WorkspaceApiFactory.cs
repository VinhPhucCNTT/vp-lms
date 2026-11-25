using System.Security.Cryptography;
using Backend.Api.Core.Types;
using Backend.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sqids;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Backend.Api.Tests.Integration;

public sealed class WorkspaceApiFactory : IAsyncLifetime
{
    private WebApplicationFactory<Program>? host;
    private DirectoryInfo? storage;
    private IDbContextFactory<AppDbContext> DbFactory =>
        host!.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();

    public async Task InitializeAsync()
    {
        var target = TestDatabase.Parse(Environment.GetEnvironmentVariable(TestDatabase.Variable));
        await TestDatabase.VerifyServerAsync(target);
        storage = Directory.CreateTempSubdirectory("vp-lms-integration-");
        host = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = target.ConnectionString,
                    ["FileStorage:RootPath"] = storage.FullName,
                    ["Jwt:Key"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
                    ["Jwt:Issuer"] = "vp-lms-integration",
                    ["Jwt:Audience"] = "vp-lms-integration",
                    ["Jwt:DurationInMinutes"] = "10"
                }));
        });

        try
        {
            if (host.Services.GetRequiredService<IWebHostEnvironment>().EnvironmentName != "Testing")
                throw new InvalidOperationException("The API test host must use the Testing environment.");
            await using var db = await OpenDbAsync();
            TestDatabase.VerifyEffectiveTarget(target, db.Database.GetConnectionString());
            await db.Database.MigrateAsync();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    internal Task<AppDbContext> OpenDbAsync() => DbFactory.CreateDbContextAsync();

    internal string Encode(long id) => host!.Services.GetRequiredService<SqidsEncoder<long>>().Encode(id);

    internal HttpClient CreateClient() => host!.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    internal async Task<HttpClient> LoginAsync(string email, string password)
    {
        var client = CreateClient();
        try
        {
            using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
            response.EnsureSuccessStatusCode();
            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (host is not null)
        {
            await host.DisposeAsync();
            host = null;
        }
        storage?.Delete(recursive: true);
        storage = null;
    }
}

[CollectionDefinition("PostgreSQL API", DisableParallelization = true)]
public sealed class PostgreSqlApiCollection : ICollectionFixture<WorkspaceApiFactory>;
