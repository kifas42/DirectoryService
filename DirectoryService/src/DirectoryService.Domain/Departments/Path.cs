using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed record Path
{
    private const char PATH_SEPARATOR = '.';
    public string Value { get; }

    private Path(string path)
    {
        Value = path;
    }

    public static Result<Path, Error> CreateFromStringPath(string path)
    {
        string[] parts = path.Split(PATH_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
        var identifiers = new List<Identifier>(parts.Length);
        foreach (string part in parts)
        {
            var createResult = Identifier.Create(part);
            if (createResult.TryGetValue(out Identifier? identifier))
            {
                identifiers.Add(identifier);
            }
            else
            {
                return createResult.Error;
            }
        }

        return Create(identifiers.ToArray());
    }

    public static Result<Path, Error> Create(params Identifier[] path)
    {
        if (path.Length == 0)
            return GeneralErrors.ValueIsEmpty("path");
        return
            new Path(
                string.Join(PATH_SEPARATOR, path.Select(id => id.Value)));
    }

    public IReadOnlyList<Identifier> ToIdentifierArray()
    {
        string[] paths = Value.Split(PATH_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
        return paths.Select(id => Identifier.Create(id).Value).ToList();
    }
}