namespace Shared;

public class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "value";
        return Error.Validation("value.is.invalid", $"{label} is invalid", name);
    }

    public static Error ValueIsEmpty(string? name)
    {
        string label = name ?? "value";
        return Error.Validation("value.is.empty", $"{label} cannot be empty", name);
    }

    public static Error NotFound(Guid? id, string? name = null)
    {
        string forId = id == null ? string.Empty : $"по Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена {forId}", id);
    }

    public static Error LenghtIsInvalid(string? name = null, int? min = null, int? max = null)
    {
        string label = $"{name ?? "value"} has invalid length";
        if (min != null && max == null)
        {
            label = $"{name ?? "value"} must be more then {min.Value} characters";
        }
        else if (min == null && max != null)
        {
            label = $"{name ?? "value"} must be less then {max.Value} characters";
        }
        else if (min != null && max != null)
        {
            label = $"{name ?? "value"} must be between {min.Value} and {max.Value} characters";
        }

        return Error.Validation("lenght.is.invalid", label, name);
    }
}