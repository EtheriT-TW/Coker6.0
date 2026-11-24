namespace EtheriT.Coker.Application.Shared.Dto.ArticleTemplate
{
    // 目錄編輯頁「文章樣板」下拉選單的一個選項。
    public sealed record ArticleTemplateInfoDto(string Key, string Name, string Description);

    // 樣板中一個可填值的欄位（不含 section 與 Map）。
    // Type 一律小寫，例如 pagetitle、string、textarea、link、list、image、gallery。
    public sealed record ArticleTemplateFieldDto(
        string Key,
        string Label,
        string Type,
        bool Required,
        string? Section,
        int? MaxCount);

    // 套用前彈窗要顯示的數字：目錄內文章總數、可套用、略過的原因。
    public sealed record ArticleTemplateChangeInfoDto(
        string TemplateKey,
        string TemplateName,
        int TotalCount,
        int ApplicableCount,
        int NoDataCount,
        int NoPermissionCount,
        IReadOnlyList<string> MissingFieldLabels);

    public sealed class ArticleTemplateApplyInputDto
    {
        public long DirectoryId { get; set; }
    }

    public sealed record ArticleTemplateApplyFailureDto(long ArticleId, string Title, string Error);

    // 背景套用完成後存進 BackgroundTasks.ResultJson。
    public sealed record ArticleTemplateApplyResultDto(
        string TemplateKey,
        int AppliedCount,
        int NoDataCount,
        int NoPermissionCount,
        IReadOnlyList<ArticleTemplateApplyFailureDto> Failures);
}