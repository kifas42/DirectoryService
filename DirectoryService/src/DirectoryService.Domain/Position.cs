using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain;

public class Position : Shared.Entity
{
    public const int MAX_LOW_LENGTH = 100;

    // ef core
    private Position()
    {
    }

    private Position(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public static Result<Position, string> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return $"name cannot be null or empty";

        if (name.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.TEXT_100)
            return $"name must be between {Constants.MIN_NAME_TEXT_LENGTH} and {Constants.TEXT_100} characters";

        if (description is not null && description.Length > Constants.MAX_TEXT_LENGTH)
            return $"description cannot be longer than {Constants.MAX_NAME_TEXT_LENGTH} characters";

        return new Position(name, description);
    }
}