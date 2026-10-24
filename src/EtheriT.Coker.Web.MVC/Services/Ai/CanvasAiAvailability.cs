using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

// Shared short-lived readiness state validates the Key and upstream model without inference.
public sealed class CanvasAiAvailability(IHttpClientFactory clients, IConfiguration configuration, IWebHostEnvironment environment,
    ILogger<CanvasAiAvailability> logger)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private DateTimeOffset checkedUntil;
    private CanvasAiReadiness lastResult = new(false, "NotChecked");
    private Uri? lastBaseUri;
    private string? lastKey;

    public bool TryGetConnection(out Uri baseUri, out string key)
        => TryGetConnection(out baseUri, out key, out _);

    private bool TryGetConnection(out Uri baseUri, out string key, out string reason)
    {
        key = configuration["ParallelAi:ApiKey"] ?? "";
        baseUri = null!;
        reason = "DisabledInConfiguration";
        if (!configuration.GetValue<bool>("ParallelAi:Enabled")) return false;
        reason = "MissingOrInvalidApiKey";
        if (string.IsNullOrWhiteSpace(key) || key == "REPLACE_WITH_SERVER_ONLY_KEY" ||
            key.Length > 512 || key.Contains('\r') || key.Contains('\n')) return false;
        reason = "InvalidBaseUrl";
        if (!Uri.TryCreate(configuration["ParallelAi:BaseUrl"], UriKind.Absolute, out var uri) ||
            !string.IsNullOrEmpty(uri.UserInfo)) return false;
        reason = "HttpNotAllowedInCurrentEnvironment";
        if (!(uri.Scheme == Uri.UriSchemeHttps ||
              ((environment.IsDevelopment() || environment.IsEnvironment("Local")) &&
               uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback))) return false;
        baseUri = uri;
        reason = "Configured";
        return true;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        => (await CheckAsync(cancellationToken)).Available;

    public async Task<CanvasAiReadiness> CheckAsync(CancellationToken cancellationToken)
    {
        if (!TryGetConnection(out var baseUri, out var key, out var reason))
        {
            logger.LogWarning("Canvas AI readiness skipped: {Reason}; environment: {Environment}", reason, environment.EnvironmentName);
            return new(false, reason);
        }
        // Share the cached result of an in-flight check instead of spuriously disabling other pages.
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (lastBaseUri != baseUri || lastKey != key)
            {
                checkedUntil = default;
                lastBaseUri = baseUri;
                lastKey = key;
            }
            if (DateTimeOffset.UtcNow < checkedUntil) return lastResult with { Cached = true };
            lastResult = new(false, "NotReady");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // Parallel allows 5 seconds for Ollama; leave time for the Gateway response.
            timeout.CancelAfter(TimeSpan.FromSeconds(8));
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, "health/ready"));
                request.Headers.Add("X-Api-Key", key);
                using var response = await clients.CreateClient("ParallelAi").SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                lastResult = new(false, (int)response.StatusCode switch {
                    401 => "ApiKeyRejected", 403 => "UpstreamForbidden", 404 => "ReadinessEndpointNotFound",
                    301 or 302 or 307 or 308 => "ReadinessRedirectNotAllowed", 503 => "OllamaOrModelNotReady",
                    _ => "UnexpectedHttpStatus"
                }, (int)response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    // Bound probe responses as well as the time spent reading their bodies.
                    await response.Content.LoadIntoBufferAsync(16384, timeout.Token);
                    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
                    var ready = json.RootElement.TryGetProperty("status", out var status) &&
                        status.ValueKind == JsonValueKind.String &&
                        string.Equals(status.GetString(), "Ready", StringComparison.OrdinalIgnoreCase);
                    lastResult = new(ready, ready ? "Ready" : "UnexpectedReadinessBody", (int)response.StatusCode);
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            { lastResult = new(false, "ReadinessTimeout"); }
            catch (HttpRequestException) { lastResult = new(false, "ConnectionOrTlsFailed"); }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or IOException)
            { lastResult = new(false, "InvalidReadinessResponse"); }
            checkedUntil = DateTimeOffset.UtcNow.AddSeconds(lastResult.Available ? 30 : 15);
            logger.LogInformation("Canvas AI readiness: {Reason}; HTTP: {HttpStatus}; environment: {Environment}",
                lastResult.Reason, lastResult.HttpStatus, environment.EnvironmentName);
            return lastResult;
        }
        finally { gate.Release(); }
    }

    public async Task MarkUnavailableAsync()
    {
        await gate.WaitAsync();
        try { lastResult = new(false, "PreviousChatFailed"); checkedUntil = DateTimeOffset.UtcNow.AddSeconds(15); }
        finally { gate.Release(); }
    }
}

public sealed record CanvasAiReadiness(bool Available, string Reason, int? HttpStatus = null, bool Cached = false);
