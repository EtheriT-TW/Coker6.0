using EtheriT.Coker.Web.Platform.Models;
using System.Text.Json;

namespace EtheriT.Coker.Web.Platform.Services;

public sealed class ViteManifestService(IWebHostEnvironment environment)
{
    private const string EntryName = "index.html";

    public ViteEntryAssets GetEntryAssets()
    {
        var manifestPath = Path.Combine(
            environment.ContentRootPath,
            "FrontendResources",
            "manifest.json");

        if (!File.Exists(manifestPath))
        {
            return new ViteEntryAssets(null, []);
        }

        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        if (!manifest.RootElement.TryGetProperty(EntryName, out var entry) ||
            !entry.TryGetProperty("file", out var file))
        {
            return new ViteEntryAssets(null, []);
        }

        var styles = entry.TryGetProperty("css", out var css)
            ? css.EnumerateArray()
                .Select(item => $"/dist/{item.GetString()}")
                .ToArray()
            : [];

        return new ViteEntryAssets(
            $"/dist/{file.GetString()}",
            styles);
    }
}
