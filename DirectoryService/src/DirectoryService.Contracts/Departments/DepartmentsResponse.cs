namespace DirectoryService.Contracts.Departments;

public class GetDepartmentDto
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string Identifier { get; set; } = null!;

    public string Path { get; set; } = null!;

    public int Depth { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public List<GetDepartmentDto> Children { get; set; } = [];

    public bool HasMoreChildren { get; set; }
}