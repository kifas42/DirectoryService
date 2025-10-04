using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Department;

public class Department : Shared.Entity
{
    // ef core
    private Department()
    {
    }

    private Department(string name, Identifier identifier, Department? parent, Path path, short depth,
        IEnumerable<DepartmentPosition> positions,
        IEnumerable<DepartmentLocation> locations)
    {
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = depth;
        _positions.AddRange(positions);
        _locations.AddRange(locations);
        Update();
    }

    public string Name { get; private set; } = string.Empty;

    public Identifier Identifier { get; private set; } = null!;

    public Department? Parent { get; private set; }

    public Path Path { get; private set; } = null!;

    public short Depth { get; private set; }

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    private readonly List<DepartmentPosition> _positions = [];

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    private readonly List<DepartmentLocation> _locations = [];

    public static Result<Department, Error> Create(string name, Identifier identifier, Department? parent, short depth,
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

        var updatePathResult = UpdatePath(parent, identifier);
        if (updatePathResult.IsFailure) return updatePathResult.Error;

        return new Department(name.Trim(), identifier, parent, updatePathResult.Value, depth, positions, locations);
    }

    public Result<Department, Error> SetParent(Department parent)
    {
        if (parent.Id == Id) return Error.Conflict(null, "parent cannot be a child himself");

        var updatePathResult = UpdatePath(Parent, Identifier);
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
        var updatePathResult = UpdatePath(Parent, identifier);
        if (updatePathResult.IsFailure)
            return updatePathResult.Error;

        Path = updatePathResult.Value;
        Identifier = identifier;
        Update();
        return identifier;
    }

    private static Result<Path, Error> UpdatePath(Department? parent, Identifier identifier)
    {
        var identifiersChain = parent?.GetIdentifiersChain() ?? [];
        identifiersChain.Add(identifier);
        var createPathResult = Path.Create(identifiersChain.ToArray());
        return createPathResult;
    }

    private List<Identifier> GetIdentifiersChain()
    {
        if (Parent == null)
        {
            return [Identifier];
        }

        var list = Parent.GetIdentifiersChain();
        list.Add(Identifier);
        return list;
    }
}