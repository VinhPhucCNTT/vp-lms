using Backend.Data;
using Backend.Core.Common;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using Sqids;

namespace Backend.Services.Content;

public class CodeProblemService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async Task GetProblemByIdAsync(long resourceId) { }

    public async Task CreateProblemAsync() { }

    public async Task UpdateProblemAsync() { }

    public async Task SubmitSolutionAsync() { }
}
