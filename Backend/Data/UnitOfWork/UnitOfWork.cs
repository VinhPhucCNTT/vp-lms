namespace Backend.Data.UnitOfWork;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private readonly AppDbContext _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }

    public Task<int> SaveHardChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveHardChangesAsync(ct);
    }
}
