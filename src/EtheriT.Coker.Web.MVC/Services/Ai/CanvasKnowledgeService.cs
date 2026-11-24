using System.Security.Cryptography;
using System.Data.Common;
using EtheriT.Coker.Application.Shared.Ai;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

// No arbitrary network/file tool is exposed to the model. Only deployed public assets
// and their local bundle inputs are indexed. Model memory is source citations, not prose.
public sealed class CanvasKnowledgeService(IWebHostEnvironment environment, IConfiguration configuration, IAiMemoryStore memory, ILogger<CanvasKnowledgeService>? logger = null)
{
    private const int Schema = 2;
    private const int MaxFileBytes = 1_000_000;

    public async Task<CanvasKnowledgeContext> PrepareAsync(CanvasAiRequest input, bool expand, CancellationToken token)
    {
        input.KnowledgeExpanded |= expand;
        var sources = new List<CanvasKnowledgeSource>();
        var unavailable = new List<string>();
        var tokens = input.SourceQueries.Take(8).Concat(Classes(input.Html).Take(80)).ToHashSet(StringComparer.Ordinal);
        var remaining = 5_000_000;
        var persistenceAvailable = true;
        foreach (var asset in input.Resources.Distinct(StringComparer.Ordinal).Take(32))
        {
            token.ThrowIfCancellationRequested();
            var resolved = Resolve(asset);
            if (resolved == null) { unavailable.Add(asset); continue; }
            var (root, relative, bundleFile) = resolved.Value;
            var runtime = await ReadPublicFile(root, relative, token);
            if (runtime == null) { unavailable.Add(asset); continue; }
            var runtimeHash = Hash(runtime);
            var runtimeTokens = Regex.Matches(runtime, @"[a-zA-Z_][a-zA-Z0-9_-]{0,99}").Select(m => m.Value).ToHashSet(StringComparer.Ordinal);
            string[] bundleInputs;
            try { bundleInputs = await BundleInputs(root, relative, bundleFile, token); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            { unavailable.Add(asset + " [manifest unavailable]"); bundleInputs = [relative]; }
            var inputs = bundleInputs.Prepend(relative).Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var path in inputs.Take(64))
            {
                if (sources.Count >= 80 || remaining <= 0) break;
                var text = path == relative ? runtime : await ReadPublicFile(root, path, token);
                if (text == null) { unavailable.Add(asset + " [source unavailable]"); continue; }
                remaining -= Encoding.UTF8.GetByteCount(text);
                if (remaining < 0) break;
                var sourceHash = Hash(text);
                var key = Hash(root + "\n" + relative + "\n" + path);
                var record = await ReadAsync<SourceRecord>(input, "system", key, token);
                if (record?.Schema != Schema || record.SourceHash != sourceHash || record.RuntimeHash != runtimeHash || record.Text != text ||
                    record.Classes == null || record.Evidence == null || record.Facts == null)
                {
                    record = new(Schema, sourceHash, runtimeHash, text, ExtractClasses(text, path.EndsWith(".js", StringComparison.OrdinalIgnoreCase)), [], [], DateTimeOffset.UtcNow);
                    persistenceAvailable &= await WriteAsync(input, "system", key, record, token);
                }
                // A class from a modified source is not executable until present in the deployed bundle.
                var usableClasses = record.Classes.Where(runtimeTokens.Contains)
                    .Take(4000).ToArray();
                var newerSource = path != relative && File.GetLastWriteTimeUtc(Path.Combine(root, path)) > File.GetLastWriteTimeUtc(Path.Combine(root, relative));
                sources.Add(new(key, asset, path, sourceHash, runtimeHash, usableClasses, record.Facts,
                    SelectEvidence(record, tokens, input.SourceQueries, expand), record.Evidence.Length > 0 ? "source-cited-not-runtime-verified" : "needs-analysis", newerSource));
            }
        }
        // A website analysis is never written to the shared source namespace. Only the
        // exact current authorized CSS fingerprint can retrieve its preceding analysis.
        var cssHash = Hash(input.Css);
        var siteKey = Hash(input.WebsiteId + "\n" + input.PermissionScope + "\n" + cssHash);
        var siteClasses = ExtractClasses(input.Css);
        if (input.WebsiteId > 0 && input.PermissionScope.Length > 0 && input.Css.Length > 0)
        {
            var scope = "site-" + input.WebsiteId;
            var snapshot = await ReadAsync<SourceRecord>(input, scope, siteKey, token);
            if (snapshot == null || snapshot.Schema != Schema || snapshot.SourceHash != cssHash || snapshot.RuntimeHash != cssHash ||
                snapshot.Text != input.Css || snapshot.Facts == null || snapshot.Evidence == null)
            {
                snapshot = new(Schema, cssHash, cssHash, input.Css, siteClasses, [], [], DateTimeOffset.UtcNow);
                persistenceAvailable &= await WriteAsync(input, scope, siteKey, snapshot, token);
            }
            sources.Add(new(siteKey, "current-authorized-css", "current.css", cssHash, cssHash, siteClasses, snapshot.Facts,
                SelectEvidence(snapshot, tokens, input.SourceQueries, true), snapshot.Evidence.Length > 0 ? "source-cited-not-runtime-verified" : "needs-analysis", MemoryScope: scope));
        }
        input.KnowledgeClasses = sources.SelectMany(s => s.Classes).Concat(siteClasses).ToHashSet(StringComparer.Ordinal);
        input.KnowledgeSourceKeys = sources.Select(s => s.Key).ToHashSet(StringComparer.Ordinal);
        input.KnowledgeSources = sources;
        var preference = input.MemoryUserId.Length == 0 ? null : await ReadAsync<ReplyPreference>(input, 
            "person-" + Hash(input.WebsiteId + "\n" + input.MemoryUserId), "reply", token);
        if (string.IsNullOrEmpty(input.ConversationLocale) && preference?.Locale is { Length: > 0 and <= 80 } &&
            Regex.IsMatch(preference.Locale, @"\A[a-zA-Z]{2,8}(?:-[a-zA-Z0-9]{1,8})*\z") && preference.Locale.Length <= 80)
        {
            input.ConversationLocale = preference.Locale;
            input.ReplyLocale = preference.Locale;
        }
        return new(sources.Where(s => s.Evidence.Length > 0)
                .OrderByDescending(s => s.Classes.Where(tokens.Contains).Sum(c => c.Length * c.Length) +
                    (input.SourceQueries.Any(q => s.Evidence.Any(e => e.Text.Contains(q, StringComparison.Ordinal))) ? 100000 : 0))
                .ThenBy(s => s.Path.Contains(".min.") ? 1 : 0).Take(12)
                .Select(s => new CanvasKnowledgeExcerpt(s.Key, s.Asset, s.Path, s.SourceHash, s.RuntimeHash, s.Status, s.Classes.Take(100).ToArray(), s.Facts, s.Evidence, s.SourceMayBeNewerThanRuntime,
                    s.MemoryScope == "system" ? "system" : "website")).ToArray(),
            unavailable.Distinct().Take(32).ToArray(), siteClasses.Take(200).ToArray(),
            "Source citations are not runtime verification. Missing/truncated sources mean unknown capability, not unsupported. Current permissions, page and runtime override memory.", persistenceAvailable);
    }

    public async Task RememberAsync(CanvasAiRequest input, string plan, CancellationToken token)
    {
        using var json = JsonDocument.Parse(plan);
        if (!json.RootElement.TryGetProperty("sourceEvidence", out var evidence) || evidence.ValueKind != JsonValueKind.Array) return;
        foreach (var item in evidence.EnumerateArray().Take(8))
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("key", out var key) || key.ValueKind != JsonValueKind.String ||
                !input.KnowledgeSourceKeys.Contains(key.GetString()!) || !item.TryGetProperty("start", out var start) || start.ValueKind != JsonValueKind.Number || !start.TryGetInt32(out var first) ||
                !item.TryGetProperty("end", out var end) || end.ValueKind != JsonValueKind.Number || !end.TryGetInt32(out var last)) continue;
            var source = input.KnowledgeSources.Single(s => s.Key == key.GetString());
            // Only evidence actually supplied to the model can be promoted. No model text,
            // request, page HTML, customer content or proposed edits enter shared memory.
            if (!source.Evidence.Any(e => first >= e.Start && last <= e.End) || first < 1 || last < first || last - first > 100) continue;
            try
            {
                for (var attempt = 0; attempt < 3; attempt++)
                {
                var record = await ReadCore<SourceRecord>(input, source.MemoryScope, source.Key, token);
                if (record != null && record.SourceHash == source.SourceHash && record.RuntimeHash == source.RuntimeHash)
                {
                    var facts = record.Facts.ToList();
                    var block = string.Join('\n', record.Text.Replace("\r\n", "\n").Split('\n').Skip(first - 1).Take(last - first + 1));
                    if (item.TryGetProperty("facts", out var observations) && observations.ValueKind == JsonValueKind.Array)
                        foreach (var fact in observations.EnumerateArray().Take(6))
                        {
                            if (fact.ValueKind != JsonValueKind.Object || !fact.TryGetProperty("className", out var cls) || cls.ValueKind != JsonValueKind.String ||
                                !fact.TryGetProperty("parameter", out var param) || param.ValueKind != JsonValueKind.String ||
                                !fact.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.String) continue;
                            var className = cls.GetString()!; var parameter = param.GetString()!; var scalar = value.GetString()!;
                            if (!source.Classes.Contains(className) || !Regex.IsMatch(parameter, @"\A[a-zA-Z_][a-zA-Z0-9_-]{0,49}\z") ||
                                !Regex.IsMatch(scalar, @"\A[a-zA-Z0-9_.-]{1,80}\z") || !block.Contains("." + className, StringComparison.Ordinal) ||
                                !Regex.IsMatch(block, @"(?<![\w-])" + Regex.Escape(parameter) + @"(?![\w-])") ||
                                !Regex.IsMatch(block, @"(?<![\w.-])" + Regex.Escape(scalar) + @"(?![\w.-])")) continue;
                            facts.Add(new(className, parameter, scalar, first, last, "source-analysis-not-runtime-verified"));
                        }
                    var applied = await WriteCore(input, source.MemoryScope, source.Key, record with {
                        Evidence = record.Evidence.Append(new(first, last, "")).DistinctBy(e => (e.Start, e.End)).TakeLast(12).ToArray(),
                        Facts = facts.Distinct().TakeLast(24).ToArray()
                    }, token);
                    if (applied) break;
                }
                else break;
                }
            }
            catch (Exception ex) when (ex is DbException or DbUpdateException)
            { logger?.LogWarning("AI source memory could not be persisted to the database; source analysis remains unverified and will be retried."); }
        }
    }

    public Task RememberReplyAsync(CanvasAiRequest input, CancellationToken token) => input.MemoryUserId.Length == 0
        ? Task.CompletedTask : WriteAsync(input, "person-" + Hash(input.WebsiteId + "\n" + input.MemoryUserId), "reply", new ReplyPreference(input.ReplyLocale), token);

    private (string Root, string Relative, string? Bundle)? Resolve(string asset)
    {
        if (!Regex.IsMatch(asset, @"\A/[a-zA-Z0-9_./-]+\.(?:js|css)\z", RegexOptions.IgnoreCase)) return null;
        if (!asset.StartsWith('/') || asset.StartsWith("//") || asset.Contains('\\') || asset.Contains('%') || asset.Contains('?') || asset.Contains('#') ||
            asset.Split('/').Any(p => p is "." or "..") || !(asset.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || asset.EndsWith(".css", StringComparison.OrdinalIgnoreCase))) return null;
        if (asset.StartsWith("/Shared/", StringComparison.OrdinalIgnoreCase))
        {
            var root = configuration["VirtualDirectory:Shared"];
            if (string.IsNullOrEmpty(root) || !Path.IsPathFullyQualified(root)) return null;
            var project = Directory.GetParent(Path.GetFullPath(root))?.Parent?.FullName;
            return (root, asset[8..], project == null ? null : Path.Combine(project, "bundles.json"));
        }
        if (asset.StartsWith("/js/Grapes/", StringComparison.OrdinalIgnoreCase) || asset.StartsWith("/css/Grapes/", StringComparison.OrdinalIgnoreCase) || asset.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase))
            return (environment.WebRootPath, asset[1..], Path.Combine(environment.ContentRootPath, "bundles.json"));
        // Layout/upload paths require site-specific authorization; use the current scoped CSS
        // instead of reading another site's files from a virtual directory.
        return null;
    }

    private static async Task<string[]> BundleInputs(string root, string relative, string? bundle, CancellationToken token)
    {
        var inputs = new List<string>();
        if (bundle != null && File.Exists(bundle))
        {
            var bytes = new FileInfo(bundle).Length;
            if (bytes <= MaxFileBytes)
            {
                using var json = JsonDocument.Parse((await File.ReadAllTextAsync(bundle, token)).TrimStart('\uFEFF'));
                foreach (var category in new[] { "scripts", "styles" })
                {
                    if (!json.RootElement.TryGetProperty(category, out var entries) || entries.ValueKind != JsonValueKind.Array) continue;
                    foreach (var entry in entries.EnumerateArray())
                    {
                        if (!entry.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.String ||
                            !entry.TryGetProperty("input", out var input) || input.ValueKind != JsonValueKind.Array) continue;
                        var basePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(bundle)!, "wwwroot"));
                        var deployed = Path.GetFullPath(Path.Combine(basePath, output.GetString()!));
                        if (!string.Equals(deployed, Path.GetFullPath(Path.Combine(root, relative)), StringComparison.OrdinalIgnoreCase)) continue;
                        foreach (var value in input.EnumerateArray().Take(128))
                        {
                            if (value.ValueKind != JsonValueKind.String) continue;
                            var pattern = value.GetString()!;
                            if (pattern.StartsWith('!')) continue;
                            var absolute = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(bundle)!, pattern));
                            var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                            if (!absolute.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                            if (pattern.Contains('*'))
                            {
                                var star = absolute.IndexOf('*');
                                var folder = absolute[..(absolute.LastIndexOf(Path.DirectorySeparatorChar, star) + 1)];
                                if (Directory.Exists(folder))
                                    inputs.AddRange(Directory.EnumerateFiles(folder, relative.EndsWith(".js") ? "*.js" : "*.css", new EnumerationOptions { RecurseSubdirectories = pattern.Contains("**"), AttributesToSkip = FileAttributes.ReparsePoint, IgnoreInaccessible = true })
                                        .Take(64).Select(p => Path.GetRelativePath(root, p)));
                            }
                            else inputs.Add(Path.GetRelativePath(root, absolute));
                        }
                    }
                }
            }
        }
        if (inputs.Count == 0 && relative.Contains(".min."))
        {
            var original = relative.Replace(".min.", ".", StringComparison.OrdinalIgnoreCase);
            if (File.Exists(Path.Combine(root, original))) inputs.Add(original);
        }
        return inputs.Count == 0 ? [relative] : inputs.Distinct(StringComparer.OrdinalIgnoreCase).Take(64).ToArray();
    }

    private static async Task<string?> ReadPublicFile(string root, string relative, CancellationToken token)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(root, relative));
        try
        {
            if (!path.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(path) || new FileInfo(path).Length > MaxFileBytes) return null;
            for (var part = new FileInfo(path) as FileSystemInfo; part != null && part.FullName.Length >= fullRoot.Length; part = part is FileInfo f ? f.Directory : ((DirectoryInfo)part).Parent)
                if ((part.Attributes & FileAttributes.ReparsePoint) != 0) return null;
            if (!(path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".css", StringComparison.OrdinalIgnoreCase))) return null;
            return await File.ReadAllTextAsync(path, token);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
    }

    private static CanvasSourceRange[] SelectEvidence(SourceRecord record, HashSet<string> tokens, string[] queries, bool expand)
    {
        var lines = record.Text.Replace("\r\n", "\n").Split('\n');
        var found = new List<(int Start, int End)>();
        foreach (var query in queries.Take(8))
        {
            var index = Array.FindIndex(lines, line => line.Contains(query, StringComparison.Ordinal));
            if (index >= 0) found.Add((Math.Max(1, index - 5), Math.Min(lines.Length, index + 90)));
        }
        if (!expand) found.AddRange(record.Evidence.Where(e => tokens.Any(t => string.Join('\n', lines.Skip(e.Start - 1).Take(e.End - e.Start + 1)).Contains(t, StringComparison.Ordinal))).Select(e => (e.Start, e.End)));
        for (var i = 0; i < lines.Length && found.Count < (expand ? 8 : 3) && (expand || record.Evidence.Length == 0 || found.Count == 0); i++)
            if (tokens.Any(t => lines[i].Contains(t, StringComparison.Ordinal) && Regex.IsMatch(lines[i], @"(?<![\w-])" + Regex.Escape(t) + @"(?![\w-])")) && !found.Any(r => i + 1 >= r.Start && i + 1 <= r.End))
                found.Add((Math.Max(1, i - 5), Math.Min(lines.Length, i + (expand ? 100 : 65))));
        if (found.Count == 0 && expand) found.Add((1, Math.Min(lines.Length, 80)));
        return found.Distinct().Take(expand ? 8 : 3).Select(r => {
            var text = string.Join('\n', lines.Skip(r.Start - 1).Take(r.End - r.Start + 1));
            var offset = 0;
            if (r.Start == r.End && text.Length > 6500)
            {
                var position = tokens.Select(t => text.IndexOf(t, StringComparison.Ordinal)).FirstOrDefault(p => p >= 0, 0);
                offset = Math.Max(0, position - 500);
            }
            return new CanvasSourceRange(r.Start, r.End, text.Substring(offset, Math.Min(6500, text.Length - offset)));
        }).ToArray();
    }

    private static IEnumerable<string> Classes(string html)
    {
        var doc = new HtmlDocument(); doc.LoadHtml(html);
        return doc.DocumentNode.Descendants().SelectMany(n => n.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
    internal static string[] ExtractClasses(string text, bool script = false)
    {
        // In JS, inspect string literals rather than confusing .map/.replace method calls
        // with CSS classes. The model still determines meaning from the cited source.
        var selectorText = script ? string.Join('\n', Regex.Matches(text, "\"[^\"\\r\\n]{1,1000}\"|'[^'\\r\\n]{1,1000}'").Select(m => m.Value)) : text;
        return Regex.Matches(selectorText, @"(?<![\w-])\.([a-zA-Z_][a-zA-Z0-9_-]{0,99})")
            .Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).Take(8000).ToArray();
    }
    private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static AiMemoryScope Scope(CanvasAiRequest input, string name)
    {
        if (name == "system") return new(0, null, null, "canvas", "source");
        if (name == "site-" + input.WebsiteId && input.WebsiteId > 0 && input.PermissionScope.Length > 0)
            return new(1, input.WebsiteId, null, input.PermissionScope, "source");
        if (input.WebsiteId > 0 && long.TryParse(input.MemoryUserId, out var person) && person > 0 &&
            name == "person-" + Hash(input.WebsiteId + "\n" + input.MemoryUserId))
            return new(2, input.WebsiteId, person, "canvas", "reply-locale");
        throw new InvalidOperationException("Memory scope is not authorized by the current request.");
    }
    private async Task<T?> ReadCore<T>(CanvasAiRequest input, string name, string key, CancellationToken token)
    {
        var scope = Scope(input, name);
        var document = await memory.ReadAsync(scope, key == "reply" ? Hash(key) : key, token);
        input.MemoryVersions[scope.Key + ":" + key] = document?.Token;
        if (document == null) return default;
        try { return JsonSerializer.Deserialize<T>(document.Payload); }
        catch (JsonException) { return default; }
    }
    private async Task<T?> ReadAsync<T>(CanvasAiRequest input, string name, string key, CancellationToken token)
    {
        try { return await ReadCore<T>(input, name, key, token); }
        catch (Exception ex) when (ex is DbException or DbUpdateException)
        { logger?.LogWarning("AI memory database is unavailable; persistent memory was not read."); return default; }
    }
    private async Task<bool> WriteCore<T>(CanvasAiRequest input, string name, string key, T value, CancellationToken token)
    {
        var scope = Scope(input, name);
        input.MemoryVersions.TryGetValue(scope.Key + ":" + key, out var expected);
        var result = await memory.WriteAsync(scope, key == "reply" ? Hash(key) : key,
            JsonSerializer.Serialize(value, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
            expected, long.TryParse(input.MemoryUserId, out var actor) && actor > 0 ? actor : null, token);
        if (result.Applied) input.MemoryVersions[scope.Key + ":" + key] = result.Token;
        return result.Applied;
    }
    private async Task<bool> WriteAsync<T>(CanvasAiRequest input, string name, string key, T value, CancellationToken token)
    {
        try { return await WriteCore(input, name, key, value, token); }
        catch (Exception ex) when (ex is DbException or DbUpdateException)
        { logger?.LogWarning("AI memory database is unavailable; persistent memory was not recorded."); return false; }
    }
    private sealed record SourceRecord(int Schema, string SourceHash, string RuntimeHash, string Text, string[] Classes, CanvasSourceRange[] Evidence, CanvasCapabilityFact[] Facts, DateTimeOffset IndexedAt);
    private sealed record ReplyPreference(string Locale);
}

public sealed record CanvasKnowledgeContext(CanvasKnowledgeExcerpt[] Sources, string[] Unavailable, string[] SiteClasses, string Policy, bool PersistenceAvailable);
public sealed record CanvasKnowledgeExcerpt(string Key, string Asset, string Path, string SourceHash, string RuntimeHash, string Status, string[] Classes, CanvasCapabilityFact[] Facts, CanvasSourceRange[] Evidence, bool SourceMayBeNewerThanRuntime = false, string Scope = "system");
public sealed record CanvasSourceRange(int Start, int End, string Text);
public sealed record CanvasKnowledgeSource(string Key, string Asset, string Path, string SourceHash, string RuntimeHash, string[] Classes, CanvasCapabilityFact[] Facts, CanvasSourceRange[] Evidence, string Status, bool SourceMayBeNewerThanRuntime = false, string MemoryScope = "system");
public sealed record CanvasCapabilityFact(string ClassName, string Parameter, string Value, int Start, int End, string Status);
