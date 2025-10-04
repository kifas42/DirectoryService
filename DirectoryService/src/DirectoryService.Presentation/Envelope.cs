using System.Text.Json.Serialization;
using Shared;

namespace DirectoryService.Presentation;

public class Envelope
{
    public object? Result { get; }

    public Errors? ErrorList { get; }

    public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(object? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = null) => new(result, null);

    public static Envelope Error(Errors? error) => new(null, error);
}

public class Envelope<T>
{
    public T? Result { get; }

    public Errors? ErrorList { get; }

    public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(T? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope<T> Ok(T? result = default) => new(result, null);

    public static Envelope<T> Error(Errors? error) => new(default, error);
}