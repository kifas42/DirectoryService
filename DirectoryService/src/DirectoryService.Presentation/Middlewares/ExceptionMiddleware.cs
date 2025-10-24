using System.Net;
using Shared;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

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
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var err = Error.Failure("server.internal", e.Message);

            var envelope = Envelope.Error(err);
            _logger.LogError("{}", e.Message);
            _logger.LogDebug("{}", e.StackTrace);
            await httpContext.Response.WriteAsJsonAsync(envelope);
        }
    }
}

public static class ExceptionHandlerExtension
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}