using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public sealed class Position : Shared.Entity
{
    // ef core
    private Position() { }

    private Position(PositionId id, string name, string? description,
        IEnumerable<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Description = description;

        _departments = departmentPositions.ToList();
    }

    public PositionId Id { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    private readonly List<DepartmentPosition> _departments = [];

    public static Result<Position, Error> Create(PositionId id, string name, string? description,
        IEnumerable<DepartmentPosition> departmentPositions)
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

        return new Position(id, name, description, departmentPositions);
    }
}