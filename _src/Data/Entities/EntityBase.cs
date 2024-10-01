namespace Data.Entities;

public class EntityBase
{
    public EntityBase()
    {
        Id = Ulid.NewUlid().ToGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
    
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}