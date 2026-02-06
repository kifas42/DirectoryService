using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed class Department : Shared.Entity
{
    // ef core
    private Department() { }

    private Department(
        DepartmentId id,
        string name,
        Identifier identifier,
        Department? parent,
        Path path,
        short depth,
        IEnumerable<DepartmentPosition> positions,
        IEnumerable<DepartmentLocation> locations)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = depth;
        IsActive = true;

        _positions.AddRange(positions);
        _locations.AddRange(locations);
        Update();
    }

    public DepartmentId Id { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public Identifier Identifier { get; private set; } = null!;

    public Department? Parent { get; private set; }

    public Path Path { get; private set; } = null!;

    public short Depth { get; private set; }

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    private List<DepartmentPosition> _positions = [];

    private List<DepartmentLocation> _locations = [];

    public static Result<Department, Error> Create(
        DepartmentId id,
        string name,
        Identifier identifier,
        Department? parent,
        short depth,
        IEnumerable<DepartmentPosition> positions,
        IEnumerable<DepartmentLocation> locations)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsEmpty("name");
        if (name.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid("name", Constants.MIN_NAME_TEXT_LENGTH,
                Constants.MAX_NAME_TEXT_LENGTH);
        }

        var updatePathResult = SetPath(parent, identifier);
        if (updatePathResult.IsFailure) return updatePathResult.Error;

        return new Department(id, name.Trim(), identifier, parent, updatePathResult.Value, depth, positions, locations);
    }

    public Result<Department, Error> SetParent(Department parent)
    {
        if (parent.Id == Id) return Error.Conflict(null, "parent cannot be a child himself");

        var updatePathResult = SetPath(Parent, Identifier);
        if (updatePathResult.IsFailure)
            return updatePathResult.Error;

        Path = updatePathResult.Value;
        Parent = parent;
        Update();
        return Parent;
    }

    public Result<string, Error> Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsEmpty("name");
        if (name.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid("name", Constants.MIN_NAME_TEXT_LENGTH,
                Constants.MAX_NAME_TEXT_LENGTH);
        }

        Name = name.Trim();
        Update();
        return Name;
    }

    public Result<Identifier, Error> SetIdentifier(Identifier identifier)
    {
        var updatePathResult = SetPath(Parent, identifier);
        if (updatePathResult.IsFailure)
            return updatePathResult.Error;

        Path = updatePathResult.Value;
        Identifier = identifier;
        Update();
        return identifier;
    }

    public Result<int, Error> SetLocations(IEnumerable<DepartmentLocation> locations)
    {
        try
        {
            _locations = locations.ToList();
        }
        catch (Exception e)
        {
            return Error.Failure(null, "locations cannot be empty");
        }

        Update();
        return _locations.Count;
    }

    private static Result<Path, Error> SetPath(Department? parent, Identifier identifier)
    {
        var parentPath = parent?.Path?.ToIdentifierArray().ToList() ?? [];
        parentPath.Add(identifier);
        var newPathResult = Path.Create(parentPath.ToArray());
        return newPathResult;
    }
}