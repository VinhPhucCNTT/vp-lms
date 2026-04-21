namespace Backend.Data.UnitOfWork;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<int> SaveHardChangesAsync(CancellationToken ct = default);
}
