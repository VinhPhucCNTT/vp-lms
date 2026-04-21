namespace Backend.Models.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
