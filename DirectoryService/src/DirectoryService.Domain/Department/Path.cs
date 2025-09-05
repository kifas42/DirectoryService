using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department;

public record Path
{
    public const string PATH_SEPARATOR = ".";
    public string Value { get; }

    private Path(string path)
    {
        Value = path;
    }

    public static Result<Path, string> Create(params Identifier[] path)
    {
        if (path.Length == 0)
            return "path cannot be null or empty";
        return
            new Path(
                string.Join(PATH_SEPARATOR, path.Select(id => id.Value)));
    }
}