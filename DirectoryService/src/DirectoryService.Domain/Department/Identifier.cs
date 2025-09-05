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

    public static Result Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Result.Failure("identifier cannot be null or empty");

        return EnglishLetterRegex().IsMatch(identifier)
            ? Result.Failure(
                "identifier must be only English letters and `-`.\nidentifier must be between 3 and 150 characters")
            : Result.Success(new Identifier(identifier.ToLower()));
    }
}