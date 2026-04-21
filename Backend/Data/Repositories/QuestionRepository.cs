using Backend.Models.Assessments;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class QuestionRepository(AppDbContext db) : IQuestionRepository
{
    private readonly AppDbContext _db = db;

    public async Task<AssessmentQuestion?> GetQuestionByIdAsync(Guid id)
    {
        return await _db.AssessmentQuestions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Add(AssessmentQuestion question)
    {
        _db.Add(question);
    }

    public void Update(AssessmentQuestion question)
    {
        _db.Update(question);
    }

    public void Remove(AssessmentQuestion question)
    {
        _db.Remove(question);
    }
}

