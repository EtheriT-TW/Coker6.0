namespace EtheriT.Coker.Application.SiteCrawl;

public enum SiteCrawlImportDecision
{
    Write,
    Unchanged,
    Conflict
}

public static class SiteCrawlReimport
{
    public static SiteCrawlImportDecision Decide(string sourceHash, string? previousSourceHash, string? writtenHash,
        string? html, string? css, bool firstUnmappedImport, string? draftHtml = null, string? draftCss = null,
        string? writtenDraftHash = null, bool requiresManualReview = false)
    {
        if (sourceHash == previousSourceHash) return SiteCrawlImportDecision.Unchanged;
        if (requiresManualReview && !new SiteCrawlContentSnapshot(html, css, draftHtml, draftCss).IsEmpty)
            return SiteCrawlImportDecision.Conflict;
        var hasDraft = !string.IsNullOrWhiteSpace(draftHtml) || !string.IsNullOrWhiteSpace(draftCss);
        if (writtenHash != null)
        {
            // 舊對照沒有草稿指紋，存在草稿時無法確認是否為人工修改。
            var draftMatches = writtenDraftHash == null ? !hasDraft
                : writtenDraftHash == SiteCrawlFingerprint.ComputeWritten(draftHtml, draftCss);
            return draftMatches && writtenHash == SiteCrawlFingerprint.ComputeWritten(html, css)
                ? SiteCrawlImportDecision.Write : SiteCrawlImportDecision.Conflict;
        }

        // 中途停止可能只留下 Id。若這段期間已填入內容，讓使用者決定是否要覆蓋。
        return firstUnmappedImport || !hasDraft && string.IsNullOrWhiteSpace(html) && string.IsNullOrWhiteSpace(css)
            ? SiteCrawlImportDecision.Write : SiteCrawlImportDecision.Conflict;
    }

    public static bool ShouldUpdate<T>(T source, T previousSource, T current, T lastWritten) =>
        !EqualityComparer<T>.Default.Equals(source, previousSource) && EqualityComparer<T>.Default.Equals(current, lastWritten);

    // 稽核的建立時間以主機本地時間寫入 SQL，須先轉成 UTC 才能與對照檔的時間比較。
    public static bool WasRecreated(DateTime creationTime, DateTimeOffset importedAt) =>
        creationTime.ToUniversalTime() > importedAt.UtcDateTime;
}

internal sealed record SiteCrawlContentSnapshot(string? Html, string? Css, string? SaveHtml, string? SaveCss)
{
    internal bool IsEmpty => string.IsNullOrWhiteSpace(Html) && string.IsNullOrWhiteSpace(Css)
        && string.IsNullOrWhiteSpace(SaveHtml) && string.IsNullOrWhiteSpace(SaveCss);
}
