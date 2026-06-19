using CSharpFunctionalExtensions;
using Shared;
using TimeZoneConverter;

namespace DirectoryService.Domain.Shared;

public record Timezone
{
    private Timezone(string value, TimeZoneInfo timeZoneInfo)
    {
        Value = value;
        TimeZoneInfo = timeZoneInfo;
    }

    public string Value { get; }
    public TimeZoneInfo TimeZoneInfo { get; }

    public static Result<Timezone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.RequiredField("timezone");
        }

        if (TZConvert.TryGetTimeZoneInfo(value, out TimeZoneInfo? timezone))
        {
            return new Timezone(value, timezone);
        }

        return Error.Validation(
            DomainErrorCodes.Validation.InvalidTimezone,
            "Указан неверный часовой пояс. Используйте формат IANA (например, 'Europe/Moscow')",
            "timezone");
    }
}