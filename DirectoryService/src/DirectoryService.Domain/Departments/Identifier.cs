using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed record Identifier
{
    private static readonly Regex _englishLetterRegex = new(@"^[a-zA-Z\-]{3,150}$", RegexOptions.Compiled);
    private Identifier(string identifier) => Value = identifier;

    public string Value { get; }

    public static Result<Identifier, Error> Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return GeneralErrors.RequiredField("identifier");
        }

        return Error.Validation(
            DomainErrorCodes.Validation.InvalidIdentifierFormat,
            "Идентификатор должен содержать только английские буквы и дефис (-), длина от 3 до 150 символов",
            "identifier");
    }
}