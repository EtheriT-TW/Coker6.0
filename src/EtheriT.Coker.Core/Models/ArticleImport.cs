using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    // 一次 Excel 匯入的設定；進度與狀態記在 BackgroundTasks（FK_BackgroundTaskId）。
    public class ArticleImportBatch : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_DirectoryId { get; set; }

        // AI 填欄位＋下載圖片的任務；完成後停在「等待確認」。
        public long FK_BackgroundTaskId { get; set; }

        // 使用者在預覽頁按確認後，建立／更新文章的任務。
        public long? FK_ConfirmTaskId { get; set; }

        // 匯入當下目錄使用的樣板；途中改目錄樣板也不影響這批。
        [StringLength(100)]
        public string TemplateKey { get; set; } = "";

        [StringLength(255)]
        public string SourceFileName { get; set; } = "";

        // 使用者勾選要匯入（送給 AI）的 Excel 欄名，JSON 陣列。
        public string ColumnsJson { get; set; } = "[]";

        public List<ArticleImportRow> Rows { get; set; } = new();
    }

    // Excel 的一列＝一篇文章。
    public class ArticleImportRow : FullAuditedEntity
    {
        public long FK_ArticleImportBatchId { get; set; }

        // Excel 列號（標題是第 1 列，資料從第 2 列開始），結果畫面用來對照原始檔。
        public int RowNumber { get; set; }

        public ArticleImportRowStatusEnum Status { get; set; }

        // 勾選欄位的原始值，JSON 物件 {"欄名":"值"}。
        public string SourceJson { get; set; } = "{}";

        // AI 填好的欄位資料（預覽頁可修改），格式同 Article.FieldDataJson。
        public string? FieldDataJson { get; set; }

        // 這列下載的檔案 Id（JSON 陣列），預覽時取消這列要刪掉。
        public string? DownloadedFilesJson { get; set; }

        // 比對到的同名文章（確認後更新它）；確認寫入後為建立或更新的文章。
        public long? FK_ArticleId { get; set; }

        [StringLength(1000)]
        public string? Error { get; set; }

        public ArticleImportBatch? Batch { get; set; }
    }
}