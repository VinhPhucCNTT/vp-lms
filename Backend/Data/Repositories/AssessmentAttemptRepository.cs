using Backend.Models.Assessments;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class AssessmentAttemptRepository(AppDbContext db) : IAssessmentAttemptRepository
{
    private readonly AppDbContext _db = db;

    public async Task<AssessmentAttempt?> GetByIdAsync(Guid id)
    {
        return await _db.AssessmentAttempts.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AssessmentAttempt>> GetByStudentIdAsync(Guid studentId)
    {
        return await _db.AssessmentAttempts
            .Where(x => x.StudentId == studentId)
            .ToListAsync();
    }

    public async void AddAsync(AssessmentAttempt attempt)
    {
        _db.AssessmentAttempts.Add(attempt);
    }

    public async void Update(AssessmentAttempt attempt)
    {
        _db.AssessmentAttempts.Update(attempt);
    }
}
