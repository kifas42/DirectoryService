using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department;

public partial record Identifier
{
    private Identifier(string identifier)
    {
        Value = identifier;
    }

    [GeneratedRegex(@"^[a-zA-Z\-]{3,150}$")]
    private static partial Regex EnglishLetterRegex();

    public string Value { get; }

    public static Result<Identifier, string> Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return "identifier cannot be null or empty";

        return EnglishLetterRegex().IsMatch(identifier)
            ? "identifier must be only English letters and `-`.\nidentifier must be between 3 and 150 characters"
            : new Identifier(identifier.ToLower());
    }
}