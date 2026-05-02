using System.Net;
using Shared;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}", e.Message);
            _logger.LogDebug("{Trace}", e.StackTrace);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            Error err = Error.Failure("exception.middleware", e.Message);

            Envelope envelope = Envelope.Fail(err);
            await httpContext.Response.WriteAsJsonAsync(envelope);
        }
    }
}

public static class ExceptionHandlerExtension
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionMiddleware>();
}