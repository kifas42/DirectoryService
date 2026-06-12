using System.Text.Json.Serialization;

namespace Shared;

public record ErrorMessage(ErrorCode Code, string Message, string? InvalidField);

public sealed record Error
{
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

    public IReadOnlyList<ErrorMessage> Messages { get; } = [];
    public ErrorType Type { get; }

    public static Error Validation(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.VALIDATION);

    public static Error NotFound(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.NOT_FOUND);

    public static Error Conflict(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.CONFLICT);

    public static Error Failure(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.FAILURE);

    public static Error Validation(ErrorCode code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.VALIDATION);

    public static Error NotFound(ErrorCode code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.NOT_FOUND);

    public static Error Conflict(ErrorCode code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.CONFLICT);

    public static Error Failure(ErrorCode code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.FAILURE);
}