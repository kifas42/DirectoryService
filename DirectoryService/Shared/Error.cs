﻿namespace Shared;

public sealed record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public string? InvalidField { get; }

    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public static Error Validation(string? code, string message, string? invalidField) =>
        new(code ?? "validation.is.invalid", message, ErrorType.VALIDATION, invalidField);

    public static Error NotFound(string? code, string message, Guid? id) =>
        new(code ?? "record.not.found", message, ErrorType.NOT_FOUND);

    public static Error Conflict(string? code, string message) =>
        new(code ?? "validation.is.conflict", message, ErrorType.CONFLICT);

    public static Error Failure(string? code, string message) =>
        new(code ?? "failure", message, ErrorType.FAILURE);
}