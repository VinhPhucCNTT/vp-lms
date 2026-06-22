using AutoMapper;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Users;

public class EnrollmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper
    )
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;

    public async Task GetCourseEnrollmentsAsync(long courseId) { }

    public async Task GetUserEnrollmentsAsync(long userId) { }

    public async Task GetCurrentEnrollmentsAsync() { }

    public async Task EnrollCourseAsync(long courseId) { }

    public async Task UnEnrollCourseAsync(long courseId) { }
}
