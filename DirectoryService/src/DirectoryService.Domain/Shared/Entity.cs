namespace DirectoryService.Domain.Shared;

public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public void Activate()
    {
        Update();
        IsActive = true;
    }

    public void Deactivate()
    {
        Update();
        IsActive = false;
    }

    protected void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}