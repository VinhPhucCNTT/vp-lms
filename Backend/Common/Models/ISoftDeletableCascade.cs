namespace Backend.Common.Models;

public interface ISoftDeletableCascade
{
    bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
