using System.Text.Json;
using FluentValidation.Results;
using Shared;

namespace DirectoryService.Application.Validation;

public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult validationResult)
    {
        List<ValidationFailure> validationErrors = validationResult.Errors;

        var errors =
            from validationError in validationErrors
            let errorMessage = validationError.ErrorMessage
            let error = JsonSerializer.Deserialize<Error>(errorMessage)
            select error.Messages;

        return Error.Validation(errors.SelectMany(error => error));
    }
}