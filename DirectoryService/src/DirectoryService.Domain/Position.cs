using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain;

public sealed class Position : Shared.Entity
{
    public const int MAX_LOW_LENGTH = 100;

    // ef core
    private Position() { }

    private Position(string name, string? description)
    {
        Id = new PositionId(Guid.NewGuid());
        Name = name;
        Description = description;
    }

    public PositionId Id { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public static Result<Position, Error> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsEmpty("name");

        if (name.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.TEXT_100)
        {
            return GeneralErrors.LenghtIsInvalid("name", Constants.MIN_NAME_TEXT_LENGTH, Constants.TEXT_100);
        }

        if (description is not null && description.Length > Constants.MAX_TEXT_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid("description", max: Constants.MAX_TEXT_LENGTH);
        }

        return new Position(name, description);
    }
}