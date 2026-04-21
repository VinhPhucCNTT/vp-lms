using Backend.Models.Assessments;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class AssessmentRepository(AppDbContext db) : IAssessmentRepository
{
    private readonly AppDbContext _db = db;

    public async Task<Assessment?> GetByIdAsync(Guid id)
    {
        return await _db.Assessments.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Assessment?> GetFullContentByIdAsync(Guid id)
    {
        return await _db.Assessments
            .Include(a => a.Questions.OrderBy(q => q.OrderIndex))
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Assessment?> GetDeletedByIdAsync(Guid id)
    {
        return await _db.Assessments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted);
    }

    public void Add(Assessment assessment)
    {
        _db.Assessments.Add(assessment);
    }

    public void Update(Assessment assessment)
    {
        _db.Assessments.Update(assessment);
    }

    public void Remove(Assessment assessment)
    {
        _db.Assessments.Remove(assessment);
    }

    public void Restore(Assessment assessment)
    {
        assessment.IsDeleted = false;
        assessment.DeletedAt = null;

        _db.Assessments.Update(assessment);
    }

    public void HardDelete(Assessment assessment)
    {
        _db.Assessments.IgnoreQueryFilters();

        _db.Entry(assessment).State = EntityState.Deleted;
    }
}
