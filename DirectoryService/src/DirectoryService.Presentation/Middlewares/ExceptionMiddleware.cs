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
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request was canceled for {RequestPath}", httpContext.Request.Path);

            httpContext.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            var envelope = Envelope.Fail(Error.Failure(
                SharedErrorCodes.System.OperationCanceled,
                "Запрос был отменен"));
            await httpContext.Response.WriteAsJsonAsync(envelope);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred during {RequestMethod} {RequestPath}",
                httpContext.Request.Method,
                httpContext.Request.Path);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var err = Error.Failure(
                SharedErrorCodes.System.UnexpectedError,
                "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.");

            var envelope = Envelope.Fail(err);
            await httpContext.Response.WriteAsJsonAsync(envelope);
        }
    }
}

public static class ExceptionHandlerExtension
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionMiddleware>();
}