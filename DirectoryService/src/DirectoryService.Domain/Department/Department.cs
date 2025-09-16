using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department;

public class Department : Shared.Entity
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 150;

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

    public string Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public Path Path { get; private set; }

    public short Depth { get; private set; }

    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    private readonly List<DepartmentPosition> _positions = [];

    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    private readonly List<DepartmentLocation> _locations = [];

    public static Result<Department, string> Create(string name, Identifier identifier, Department? parent, short depth,
        IEnumerable<DepartmentPosition> positions,
        IEnumerable<DepartmentLocation> locations)
    {
        if (string.IsNullOrWhiteSpace(name)) return "name must not be null or empty";
        if (name.Length is < MIN_LENGTH or > MAX_LENGTH)
            return $"name must be between {MIN_LENGTH} and {MAX_LENGTH} characters";

        var updatePathResult = UpdatePath(parent, identifier);
        if (updatePathResult.IsFailure) return updatePathResult.Error;

        return new Department(name.Trim(), identifier, parent, updatePathResult.Value, depth, positions, locations);
    }

    public Result SetParent(Department parent)
    {
        if (parent.Id == Id) return Result.Failure("parent cannot be a child himself");

        var updatePathResult = UpdatePath(Parent, Identifier);
        if (updatePathResult.IsFailure)
            return Result.Failure(updatePathResult.Error);

        Path = updatePathResult.Value;
        Parent = parent;
        Update();
        return Result.Success();
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure("name must not be null or empty");
        if (name.Length is < MIN_LENGTH or > MAX_LENGTH)
            return Result.Failure($"name must be between {MIN_LENGTH} and {MAX_LENGTH} characters");

        Name = name.Trim();
        Update();
        return Result.Success();
    }

    public Result SetIdentifier(Identifier identifier)
    {
        var updatePathResult = UpdatePath(Parent, identifier);
        if (updatePathResult.IsFailure)
            return Result.Failure(updatePathResult.Error);

        Path = updatePathResult.Value;
        Identifier = identifier;
        Update();
        return Result.Success();
    }

    private static Result<Path, string> UpdatePath(Department? parent, Identifier identifier)
    {
        var identifiersChain = parent?.GetIdentifiersChain() ?? [];
        identifiersChain.Add(identifier);
        var createPathResult = Path.Create(identifiersChain.ToArray());
        return createPathResult.IsFailure ? createPathResult.Error : createPathResult.Value;
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