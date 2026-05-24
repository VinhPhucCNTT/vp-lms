namespace Backend.Common.Models;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
