using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Department;

public sealed record Identifier
{
    private Identifier(string identifier)
    {
        Value = identifier;
    }

    private static readonly Regex _englishLetterRegex = new Regex(@"^[a-zA-Z\-]{3,150}$", RegexOptions.Compiled);

    public string Value { get; }

    public static Result<Identifier, Error> Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return GeneralErrors.ValueIsEmpty("identifier");

        return _englishLetterRegex.IsMatch(identifier)
            ? Error.Validation(
                "validation.is.eng",
                $"identifier must be only English letters and `-`.\nidentifier must be between 3 and 150 characters",
                "identifier")
            : new Identifier(identifier.ToLower());
    }
}