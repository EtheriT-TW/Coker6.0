namespace EtheriT.Coker.Web.Platform.Middleware;

public sealed class PlatformApiControlMiddleware(RequestDelegate next)
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

        await next(context);
    }
}
