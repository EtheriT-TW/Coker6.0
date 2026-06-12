using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.Contact
{
    /// <summary>
    /// 聯絡表單匯出結果；成功時帶回檔案內容，失敗時沿用 ResponseMessageDto 的錯誤欄位。
    /// </summary>
    public class ContactExportResultDto : ResponseMessageDto
    {
        /// <summary>
        /// Excel 檔案位元組內容，只有成功匯出時才會有值。
        /// </summary>
        public byte[]? FileContents { get; set; }

        /// <summary>
        /// 瀏覽器下載時使用的檔名。
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Excel OpenXML 的 MIME type。
        /// </summary>
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// API 回應用的 HTTP 狀態碼，讓 Controller 能一致轉出錯誤結果。
        /// </summary>
        public int HttpStatusCode { get; set; } = 200;

        /// <summary>
        /// 規格定義的錯誤代碼，例如 E001、E002。
        /// </summary>
        public string? ErrorCodeKey { get; set; }

        /// <summary>
        /// 實際匯出的資料筆數，供稽核紀錄使用。
        /// </summary>
        public int ExportedCount { get; set; }

        /// <summary>
        /// 本次匯出套用的筆數上限，供前端錯誤回應同步提示文字。
        /// </summary>
        public int MaxRows { get; set; }
    }
}
