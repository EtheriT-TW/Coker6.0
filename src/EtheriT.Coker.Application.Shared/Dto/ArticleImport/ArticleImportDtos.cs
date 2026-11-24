using EtheriT.Coker.Application.Shared.Dto.ArticleTemplate;
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.Shared.Dto.ArticleImport
{
    // 匯入第一步的目錄選單：所有文章目錄都列出，不能匯入的附上原因（DisabledReason 有值＝不能選）。
    public sealed record ArticleImportDirectoryDto(
        long Id,
        string Title,
        string? TemplateKey,
        string? TemplateName,
        string? DisabledReason);

    // Excel 的一個欄位；標題含「不公開」的預設不勾選。
    public sealed record ArticleImportColumnDto(string Name, string Sample, bool Selected);

    public sealed record ArticleImportColumnsDto(
        string FileName,
        int RowCount,
        IReadOnlyList<ArticleImportColumnDto> Columns);

    public sealed record ArticleImportStartResultDto(long BatchId, long TaskId, int RowCount);

    public sealed record ArticleImportTaskDto(
        long Id,
        BackgroundTaskStatusEnum Status,
        int Progress,
        string Message,
        string? Error);

    // 這列從 Google Drive 下載的一張圖：Token 是給 AI 看的代號（image:1），下載失敗時 Error 有值。
    public sealed record ArticleImportImageDto(
        string Token,
        string Column,
        string FileName,
        long? FileId,
        string? Path,
        string? Error);

    // 預覽頁的一列。MatchedArticleId 有值＝同目錄有同標題文章，確認後會更新它。
    public sealed record ArticleImportRowDto(
        long Id,
        int RowNumber,
        ArticleImportRowStatusEnum Status,
        string Title,
        Dictionary<string, List<string>> Fields,
        IReadOnlyList<ArticleImportImageDto> Images,
        string? Error,
        long? MatchedArticleId,
        bool MatchedCanEdit,
        bool DuplicateInBatch);

    // 編輯彈窗用：多帶 Excel 原始值讓人對照。
    public sealed record ArticleImportRowDetailDto(
        ArticleImportRowDto Row,
        Dictionary<string, string> Source);

    public sealed record ArticleImportBatchDto(
        long Id,
        long DirectoryId,
        string DirectoryTitle,
        string TemplateKey,
        string TemplateName,
        string SourceFileName,
        DateTime CreationTime,
        ArticleImportTaskDto Task,
        ArticleImportTaskDto? ConfirmTask,
        IReadOnlyList<ArticleTemplateFieldDto> Fields,
        IReadOnlyList<ArticleImportRowDto> Rows);

    public sealed class ArticleImportRowUpdateDto
    {
        public long RowId { get; set; }
        public Dictionary<string, List<string>>? Fields { get; set; }
    }

    public sealed class ArticleImportConfirmDto
    {
        public long BatchId { get; set; }
        // 預覽頁取消勾選的列；這些列不建立文章，已下載的圖片移到資源回收桶。
        public List<long>? ExcludedRowIds { get; set; }
    }

    public sealed class ArticleImportBatchInputDto
    {
        public long BatchId { get; set; }
    }

    // AI 階段完成後存進 BackgroundTasks.ResultJson。
    public sealed record ArticleImportScanResultDto(long BatchId, int ReadyCount, int FailedCount);

    public sealed record ArticleImportRowFailureDto(int RowNumber, string Title, string Error);

    // 確認寫入完成後存進確認任務的 ResultJson。
    public sealed record ArticleImportConfirmResultDto(
        long BatchId,
        int CreatedCount,
        int UpdatedCount,
        int ExcludedCount,
        IReadOnlyList<ArticleImportRowFailureDto> Failures);
}