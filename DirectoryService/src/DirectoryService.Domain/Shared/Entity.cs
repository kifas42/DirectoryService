namespace DirectoryService.Domain.Shared;

public abstract class Entity
{
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

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