using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class Position : Shared.Entity
{
    public const int MIN_LOW_LENGTH = 3;
    public const int MAX_LOW_LENGTH = 100;
    public const int MAX_HIGH_LENGTH = 1000;

    private Position(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public static Result<Position, string> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return $"name cannot be null or empty";

        if (name.Length is < MIN_LOW_LENGTH or > MAX_LOW_LENGTH)
            return $"name must be between {MIN_LOW_LENGTH} and {MAX_LOW_LENGTH} characters";

        if (description is not null && description.Length > MAX_HIGH_LENGTH)
            return $"description cannot be longer than {MAX_HIGH_LENGTH} characters";

        return new Position(name, description);
    }
}