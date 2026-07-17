using System.Text.Json.Serialization;

namespace Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    VALIDATION,
    NOT_FOUND,
    AUTHENTICATION,
    AUTHORIZATION,
    CONFLICT,
    FAILURE,
}