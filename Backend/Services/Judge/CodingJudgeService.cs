using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Judge;

public class CodingJudgeService(
    IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

}
