using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Judge;

public class CodingJudgeService(
    IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

}
