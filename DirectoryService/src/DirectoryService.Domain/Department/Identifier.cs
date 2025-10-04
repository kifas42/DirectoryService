using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared;

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

    public static Result<Identifier, Error> Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return GeneralErrors.ValueIsEmpty("identifier");

        return EnglishLetterRegex().IsMatch(identifier)
            ? Error.Validation(
                "validation.is.eng",
                $"identifier must be only English letters and `-`.\nidentifier must be between 3 and 150 characters",
                "identifier")
            : new Identifier(identifier.ToLower());
    }
}