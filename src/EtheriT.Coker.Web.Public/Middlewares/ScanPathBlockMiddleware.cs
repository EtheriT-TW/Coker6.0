namespace EtheriT.Coker.Web.Public.Middlewares;

// Reject irrelevant probes before cookies, static files, authentication and MVC.
// IIS access logs retain evidence; do not amplify probes with DB/application log writes.
public sealed class ScanPathBlockMiddleware
{
    // Rooted technical endpoints observed in IIS logs; do not block generic
    // page names such as /login, /contact, /info, /tree or all of /api.
    private static readonly string[] UnsupportedEndpointRoots =
    {
        "/pms", "/containers/json", "/images/json",
        "/api/health", "/api/org", "/api/datasources", "/api/alert-notifications",
        "/api/v1/provisioning", "/api/2.0/mlflow",
        "/api/v1/auto_login", "/api/v1/validate/code", "/api/contents",
        "/_cat", "/_watcher", "/_cluster",
        "/v1/agent", "/v1/kv", "/v1/catalog", "/v2/keys",
        "/rest/v1", "/auth/v1", "/storage/v1", "/functions/v1",
        "/_next", "/_rsc", "/__rsc", "/.action", "/_middleware",
        "/api/auth", "/debug/default/view"
    };
    private readonly RequestDelegate next;

    public ScanPathBlockMiddleware(RequestDelegate next) => this.next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        // ASP.NET already decodes ordinary path escapes. Check an extra layer for
        // double-encoded probes, without parsing bodies or allocating per-segment arrays.
        if (path.Contains('%'))
        {
            try { path = Uri.UnescapeDataString(path); }
            catch (UriFormatException) { return Reject(context); }
        }
        if (IsProbe(path)) return Reject(context);
        return next(context);
    }

    private static bool IsProbe(string path)
    {
        foreach (var root in UnsupportedEndpointRoots)
        {
            if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                && (path.Length == root.Length || path[root.Length] == '/')) return true;
        }
        foreach (var segment in path.AsSpan().Split('/'))
        {
            var part = path.AsSpan()[segment];
            if (part.Equals("..", StringComparison.Ordinal) || part.Contains('\\')) return true;
            if (part.Equals(".git", StringComparison.OrdinalIgnoreCase)
                || part.Equals(".svn", StringComparison.OrdinalIgnoreCase)
                || part.Equals(".hg", StringComparison.OrdinalIgnoreCase)
                || part.Equals(".aws", StringComparison.OrdinalIgnoreCase)
                || part.Equals(".ssh", StringComparison.OrdinalIgnoreCase)
                || part.Equals("wp-admin", StringComparison.OrdinalIgnoreCase)
                || part.Equals("wp-content", StringComparison.OrdinalIgnoreCase)
                || part.Equals("wp-includes", StringComparison.OrdinalIgnoreCase)
                || part.Equals("phpmyadmin", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith(".env", StringComparison.OrdinalIgnoreCase)
                || part.Equals("web.config", StringComparison.OrdinalIgnoreCase)
                || (part.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase)
                    && part.EndsWith(".json", StringComparison.OrdinalIgnoreCase))) return true;
            // Include path-info forms such as /index.php/extra and PHP backups.
            var dot = part.IndexOf('.');
            while (dot >= 0)
            {
                var extension = part[(dot + 1)..];
                var end = extension.IndexOf('.');
                if (end >= 0) extension = extension[..end];
                if (extension.Equals("php", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals("phtml", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals("phar", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals("php3", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals("php4", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals("php5", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals("php7", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals("php8", StringComparison.OrdinalIgnoreCase)) return true;
                var nextDot = part[(dot + 1)..].IndexOf('.');
                dot = nextDot < 0 ? -1 : dot + 1 + nextDot;
            }
        }
        return false;
    }

    private static Task Reject(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentLength = 0;
        context.Response.Headers.CacheControl = "no-store";
        return Task.CompletedTask;
    }
}
