namespace Backend.Models.Common;

public interface ISoftDeletableCascade
{
    bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
