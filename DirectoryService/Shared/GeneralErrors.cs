namespace Shared;

public static class GeneralErrors
{
    public static Error RequiredField(string? name)
    {
        string label = name ?? "value";
        return Error.Validation(SharedErrorCodes.Validation.Required, $"Поле {label} является обязательным", name);
    }

    public static Error LenghtIsInvalid(string? name = null, int? min = null, int? max = null)
    {
        string label = $"{name ?? "Значение"}";
        if (min.HasValue && !max.HasValue)
        {
            return Error.Validation(
                SharedErrorCodes.Validation.StringTooShort,
                $"'{label}' должно содержать не менее {min.Value} символов", name);
        }

        if (!min.HasValue && max.HasValue)
        {
            return Error.Validation(
                SharedErrorCodes.Validation.StringTooLong,
                $"'{label}' не должно превышать {max.Value} символов",
                name);
        }

        if (min.HasValue && max.HasValue)
        {
            return Error.Validation(
                SharedErrorCodes.Validation.StringLengthOutOfBounds,
                $"'{label}' должно содержать от {min.Value} до {max.Value} символов", name);
        }

        return Error.Validation(
            SharedErrorCodes.Validation.StringLengthOutOfBounds,
            $"'{label}' имеет некорректную длину", name);
    }

    public static Error Duplicate(string fieldName, string? entityName = null)
    {
        string message = string.IsNullOrWhiteSpace(entityName)
            ? "Указаны дублирующиеся значения"
            : $"Список не должен содержать дубликаты {entityName}";

        return Error.Validation(
            SharedErrorCodes.Validation.Duplicate,
            message,
            fieldName);
    }
}