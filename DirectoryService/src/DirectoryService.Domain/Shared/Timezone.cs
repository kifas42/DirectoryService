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
        if (TZConvert.TryGetTimeZoneInfo(value, out var timezone))
        {
            return new Timezone(value, timezone);
        }

        var zones = TZConvert.KnownIanaTimeZoneNames;
        return Error.Validation(
            null,
            "Invalid timezone. Available zones:\n" + string.Join("\n", zones),
            "timeZone");
    }
}