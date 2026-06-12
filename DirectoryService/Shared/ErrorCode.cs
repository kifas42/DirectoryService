using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared;

[JsonConverter(typeof(ErrorCodeJsonConverter))]
public readonly record struct ErrorCode
{
    public string Value { get; }

    public ErrorCode(string value) => Value = value.ToLowerInvariant();

    public static implicit operator string(ErrorCode code) => code.Value;

    public override string ToString() => Value;
}

public class ErrorCodeJsonConverter : JsonConverter<ErrorCode>
{
    public override ErrorCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new ErrorCode(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, ErrorCode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}