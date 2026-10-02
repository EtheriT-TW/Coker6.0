using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>Diagnostic step 1: IIS website/HTTPS binding list only; no certificate or renewal reads.</summary>
public sealed partial class TlsCertificateCollector(ProcessRunner runner, ILogger<TlsCertificateCollector> logger,
    ProvisioningWorkerOptions options)
{
    private TlsSnapshot? cached;
    public TlsSnapshot? CurrentSnapshot => Volatile.Read(ref cached);

    public async Task<TlsSnapshot> CollectAsync(CancellationToken cancellationToken)
    {
        var started = Stopwatch.StartNew();
        logger.LogInformation("TLS step 1 started at {TimeUtc}: appcmd list site /xml", DateTime.UtcNow);
        TlsSnapshot snapshot;
        try
        {
            if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("IIS listing requires Windows.");
            var appCmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "System32", "inetsrv", "appcmd.exe");
            const int outputLimit = 2_000_000;
            var output = await runner.RunAsync(appCmd, ["list", "site", "/xml"], null,
                cancellationToken, maxOutputLength: outputLimit, xmlOutput: true);
            if (output.Length >= outputLimit) throw new InvalidOperationException("IIS website list exceeds the output limit.");
            var document = XDocument.Parse(output);
            if (document.Root?.Name.LocalName != "appcmd")
                throw new InvalidOperationException("Unexpected IIS website list response.");
            var websites = document.Descendants("SITE").Select(site =>
            {
                var name = (string?)site.Attribute("SITE.NAME")
                    ?? throw new InvalidOperationException("IIS website has no name.");
                var bindings = ((string?)site.Attribute("bindings") ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(binding => binding.StartsWith("https/", StringComparison.OrdinalIgnoreCase))
                    .Select(binding => binding[6..]).ToArray();
                var urls = bindings.Select(binding =>
                {
                    var hostSeparator = binding.LastIndexOf(':');
                    if (hostSeparator <= 0 || hostSeparator == binding.Length - 1) return null;
                    var portSeparator = binding.LastIndexOf(':', hostSeparator - 1);
                    if (portSeparator < 0) return null;
                    var host = binding[(hostSeparator + 1)..];
                    var port = binding[(portSeparator + 1)..hostSeparator];
                    return $"https://{host}{(port == "443" ? string.Empty : ":" + port)}";
                }).OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                return new TlsWebsite(name, (string?)site.Attribute("state") ?? "Unknown", bindings, urls);
            }).OrderBy(site => site.SiteName, StringComparer.OrdinalIgnoreCase).ToArray();
            snapshot = new TlsSnapshot(DateTime.UtcNow, [], null, websites);
            logger.LogInformation("TLS step 1 completed at {TimeUtc}: {Count} websites; {ElapsedMs} ms",
                DateTime.UtcNow, websites.Length, started.ElapsedMilliseconds);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TLS step 1 failed at {TimeUtc} after {ElapsedMs} ms", DateTime.UtcNow, started.ElapsedMilliseconds);
            var previous = CurrentSnapshot;
            snapshot = new TlsSnapshot(previous?.CollectedAtUtc, [], "第一步 IIS 網站清單讀取失敗：" + ex.Message,
                previous?.Websites ?? []);
        }
        // Step 2 is isolated: a store failure must not erase the successful website list.
        snapshot = snapshot with { CentralStore = CollectCentralStore(cancellationToken) };
        if (snapshot.CentralStore is { Error: null, DirectoryPath: not null } store)
            snapshot = snapshot with { CentralStore = store with { Certificates = ReadPfxCertificates(store, cancellationToken) } };
        snapshot = snapshot with { WacsLogs = await ReadWacsLogsAsync(cancellationToken) };
        snapshot = snapshot with { WacsSchedule = ReadWacsSchedule(cancellationToken) };
        Volatile.Write(ref cached, snapshot);
        return snapshot;
    }

    private IReadOnlyList<TlsPfxCertificate> ReadPfxCertificates(TlsCentralStore store, CancellationToken cancellationToken)
    {
        var results = new List<TlsPfxCertificate>();
        logger.LogInformation("TLS step 3 started at {TimeUtc}: {Count} PFX files; private keys ignored",
            DateTime.UtcNow, store.PfxFileNames.Count);
        foreach (var file in store.PfxFileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("TLS step 3 reading {FileName} at {TimeUtc}", file, DateTime.UtcNow);
            try
            {
                var path = Path.Combine(store.DirectoryPath!, file);
                if (new FileInfo(path).Length > 10_000_000)
                    throw new InvalidOperationException("PFX 超過 10 MB 讀取上限。");
                var certificates = X509CertificateLoader.LoadPkcs12CollectionFromFile(path,
                    options.TlsPfxPassword, X509KeyStorageFlags.EphemeralKeySet,
                    new Pkcs12LoaderLimits { IgnorePrivateKeys = true });
                try
                {
                    var leaves = certificates.Cast<X509Certificate2>().Where(certificate =>
                        !certificate.Extensions.OfType<X509BasicConstraintsExtension>()
                            .Any(extension => extension.CertificateAuthority)).ToArray();
                    if (leaves.Length == 0) throw new InvalidOperationException("PFX 沒有可辨識的網站憑證。");
                    foreach (var certificate in leaves)
                        results.Add(new TlsPfxCertificate(file, certificate.Subject, certificate.Issuer,
                            certificate.Thumbprint, certificate.NotBefore.ToUniversalTime(),
                            certificate.NotAfter.ToUniversalTime(), null));
                    logger.LogInformation("TLS step 3 completed {FileName}: {Count} certificates", file, leaves.Length);
                }
                finally
                {
                    foreach (var certificate in certificates) certificate.Dispose();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Do not log password or PFX contents. Failed files do not discard successful results.
                var message = ex is CryptographicException
                    ? "無法解析 PFX：請確認 Worker 的 TlsPfxPassword、檔案格式或載入限制。"
                    : ex.Message;
                logger.LogWarning("TLS step 3 failed {FileName}: {Error}", file, message);
                results.Add(new TlsPfxCertificate(file, null, null, null, null, null, message));
            }
        }
        logger.LogInformation("TLS step 3 completed at {TimeUtc}: {Count} results", DateTime.UtcNow, results.Count);
        return results;
    }

    private TlsCentralStore CollectCentralStore(CancellationToken cancellationToken)
    {
        var started = Stopwatch.StartNew();
        bool? enabled = null;
        string? directory = null;
        logger.LogInformation("TLS step 2 started at {TimeUtc}: read CCS location and enumerate PFX names only", DateTime.UtcNow);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("CCS listing requires Windows.");
            using var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var key = machine.OpenSubKey(@"SOFTWARE\Microsoft\IIS\CentralCertProvider", writable: false);
            if (key is null)
                throw new InvalidOperationException("找不到 IIS 集中式憑證設定登錄項目。");
            enabled = key.GetValue("Enabled") is int flag ? flag != 0 : null;
            directory = key.GetValue("CertStoreLocation") as string;
            if (string.IsNullOrWhiteSpace(directory))
            {
                if (enabled != false) throw new InvalidOperationException("未設定集中式憑證目錄。");
                logger.LogInformation("TLS step 2 completed: CCS disabled and no directory configured.");
                return new TlsCentralStore(DateTime.UtcNow, enabled, null, [], null);
            }
            if (!Path.IsPathFullyQualified(directory))
                throw new InvalidOperationException("集中式憑證目錄不是絕對路徑。");
            var names = new List<string>();
            foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!string.Equals(Path.GetExtension(path), ".pfx", StringComparison.OrdinalIgnoreCase)) continue;
                if (names.Count >= 10000) throw new InvalidOperationException("PFX 檔案超過 10000 筆，停止列舉。");
                names.Add(Path.GetFileName(path));
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            logger.LogInformation("TLS step 2 completed at {TimeUtc}: {Count} PFX names; {ElapsedMs} ms",
                DateTime.UtcNow, names.Count, started.ElapsedMilliseconds);
            return new TlsCentralStore(DateTime.UtcNow, enabled, directory, names, null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TLS step 2 failed at {TimeUtc} after {ElapsedMs} ms", DateTime.UtcNow, started.ElapsedMilliseconds);
            return new TlsCentralStore(null, enabled, directory, [], "第二步集中式憑證目錄讀取失敗：" + ex.Message);
        }
    }
}
