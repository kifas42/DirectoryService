namespace DirectoryService.Domain.Shared;

public abstract class Entity
{
    public bool IsActive { get; protected set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; protected set; }

    protected void Update() => UpdatedAt = DateTime.UtcNow;
}