using Backend.Models.Assignments;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class AssignmentRepository(AppDbContext db) : IAssignmentRepository
{
    private readonly AppDbContext _db = db;

    public Task<Assignment?> GetByIdAsync(Guid id)
    {
        return _db.Assignments.FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<Assignment?> GetDeletedByIdAsync(Guid id)
    {
        return _db.Assignments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted);
    }

    public void Add(Assignment assignment)
    {
        _db.Assignments.Add(assignment);
    }

    public void Update(Assignment assignment)
    {
        _db.Assignments.Update(assignment);
    }

    public void Remove(Assignment assignment)
    {
        _db.Assignments.Remove(assignment);
    }

    public void Restore(Assignment assignment)
    {
        assignment.IsDeleted = false;
        assignment.DeletedAt = null;

        _db.Assignments.Update(assignment);
    }

    public void HardDelete(Assignment assignment)
    {
        _db.Assignments.IgnoreQueryFilters();

        _db.Entry(assignment).State = EntityState.Deleted;
    }
}
