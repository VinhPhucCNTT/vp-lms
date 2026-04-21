using Backend.Models.Assignments;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class AssignmentSubmissionRepository(AppDbContext db) : IAssignmentSubmissionRepository
{
    private readonly AppDbContext _db = db;

    public async Task<AssignmentSubmission?> GetByIdAsync(Guid id)
    {
        return await _db.AssignmentSubmissions.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AssignmentSubmission>> GetByAssignmentIdAsync(Guid assignmentId)
    {
        return await _db.AssignmentSubmissions
            .Where(x => x.AssignmentId == assignmentId)
            .ToListAsync();
    }

    public void Add(AssignmentSubmission submission)
    {
        _db.AssignmentSubmissions.Add(submission);
    }

    public void Update(AssignmentSubmission submission)
    {
        _db.AssignmentSubmissions.Update(submission);
    }
}
