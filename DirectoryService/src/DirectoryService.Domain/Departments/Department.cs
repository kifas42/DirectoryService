using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;
using Entity = DirectoryService.Domain.Shared.Entity;

namespace DirectoryService.Domain.Departments;

public sealed class Department : Entity
{
    private readonly List<DepartmentPosition> _positions = [];

    private List<DepartmentLocation> _locations = [];

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

    public DepartmentId Id { get; } = null!;

    public string Name { get; private set; } = string.Empty;

    public Identifier Identifier { get; private set; } = null!;

    public Department? Parent { get; private set; }

    public Path Path { get; private set; } = null!;

    public short Depth { get; private set; }

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

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
        {
            return GeneralErrors.ValueIsEmpty("name");
        }

        if (name.Length is < Constants.MIN_NAME_TEXT_LENGTH or > Constants.MAX_NAME_TEXT_LENGTH)
        {
            return GeneralErrors.LenghtIsInvalid("name", Constants.MIN_NAME_TEXT_LENGTH,
                Constants.MAX_NAME_TEXT_LENGTH);
        }

        Result<Path, Error> updatePathResult = SetPath(parent, identifier);
        if (updatePathResult.IsFailure)
        {
            return updatePathResult.Error;
        }

        return new Department(id, name.Trim(), identifier, parent, updatePathResult.Value, depth, positions, locations);
    }

    public UnitResult<Error> SetParent(Department? parent)
    {
        if (parent != null)
        {
            if (parent.Id == Id)
            {
                return Error.Conflict("set.parent.conflict", "parent cannot be a child himself");
            }
        }

        Result<Path, Error> updatePathResult = SetPath(Parent, Identifier);
        if (updatePathResult.IsFailure)
        {
            return updatePathResult.Error;
        }

        Path = updatePathResult.Value;
        Parent = parent;
        Depth = (short)((parent?.Depth ?? 0) + 1);
        Update();
        return UnitResult.Success<Error>();
    }

    public Result<string, Error> Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GeneralErrors.ValueIsEmpty("name");
        }

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
        Result<Path, Error> updatePathResult = SetPath(Parent, identifier);
        if (updatePathResult.IsFailure)
        {
            return updatePathResult.Error;
        }

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
        catch (Exception)
        {
            return Error.Failure(string.Empty, "locations cannot be empty");
        }

        Update();
        return _locations.Count;
    }

    private static Result<Path, Error> SetPath(Department? parent, Identifier identifier)
    {
        List<Identifier> parentPath = parent?.Path.ToIdentifierArray().ToList() ?? [];
        parentPath.Add(identifier);
        Result<Path, Error> newPathResult = Path.Create(parentPath.ToArray());
        return newPathResult;
    }
}