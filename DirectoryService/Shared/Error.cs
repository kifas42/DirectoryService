using System.Text.Json.Serialization;

namespace Shared;

public record ErrorMessage(string Code, string Message, string? InvalidField);

public sealed record Error
{
    public IReadOnlyList<ErrorMessage> Messages { get; } = [];
    public ErrorType Type { get; }

    private Error(IEnumerable<ErrorMessage> messages, ErrorType type)
    {
        Messages = messages.ToArray();
        Type = type;
    }

    [JsonConstructor]
    private Error(IReadOnlyList<ErrorMessage> messages, ErrorType type)
    {
        Messages = messages.ToArray();
        Type = type;
    }

    public static Error Validation(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.VALIDATION);

    public static Error NotFound(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.NOT_FOUND);

    public static Error Conflict(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.CONFLICT);

    public static Error Failure(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.FAILURE);

    public static Error Validation(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.VALIDATION);

    public static Error NotFound(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.NOT_FOUND);

    public static Error Conflict(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.CONFLICT);

    public static Error Failure(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.FAILURE);
}