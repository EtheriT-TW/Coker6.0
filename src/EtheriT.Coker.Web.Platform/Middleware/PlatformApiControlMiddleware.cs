using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Platform.Middleware;

public sealed class PlatformApiControlMiddleware(
    RequestDelegate next,
    ILogger<PlatformApiControlMiddleware> logger)
{
    private const string RequestIdHeader = "X-Coker-Request-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var suppliedRequestId = context.Request.Headers[RequestIdHeader].FirstOrDefault();
        var requestId = Guid.TryParse(suppliedRequestId, out var parsedRequestId)
            ? parsedRequestId.ToString()
            : Guid.NewGuid().ToString();

        context.TraceIdentifier = requestId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[RequestIdHeader] = requestId;
            context.Response.Headers["Cache-Control"] = "no-store";
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            logger.LogError(
                exception,
                "Unhandled Platform API exception. RequestId={RequestId}; Method={Method}; Path={Path}",
                requestId,
                context.Request.Method,
                context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            context.Response.Headers[RequestIdHeader] = requestId;
            context.Response.Headers["Cache-Control"] = "no-store";
            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Platform API request failed.",
                    Detail = "An unexpected server error occurred. Use the requestId to locate the server log.",
                    Extensions = { ["requestId"] = requestId }
                },
                context.RequestAborted);
        }
    }
}
