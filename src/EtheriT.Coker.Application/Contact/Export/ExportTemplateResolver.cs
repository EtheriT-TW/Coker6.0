using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Contact.Export
{
    /// <summary>
    /// 匯出模板欄位中繼資料；目前先作為預留契約，未接資料表。
    /// </summary>
    public class ExportColumnMetadata
    {
        /// <summary>
        /// 對應 FromDate 欄位 key。
        /// </summary>
        public string ColumnKey { get; set; } = string.Empty;

        /// <summary>
        /// Excel 顯示欄名。
        /// </summary>
        public string ColumnTitle { get; set; } = string.Empty;

        /// <summary>
        /// 模板指定的欄位排序。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否輸出此欄位。
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 保留給未來格式化使用，例如日期或數字格式。
        /// </summary>
        public string? Format { get; set; }
    }

    /// <summary>
    /// 匯出模板解析器；客戶啟用模板資料表後，可在這裡接上 DB 查詢。
    /// </summary>
    public class ExportTemplateResolver
    {
        /// <summary>
        /// 目前規格未要求建立模板表，因此回傳空集合並改用 FromDate 動態欄位。
        /// </summary>
        public Task<IReadOnlyList<ExportColumnMetadata>> GetTemplateColumnsAsync(long formTypeId)
        {
            IReadOnlyList<ExportColumnMetadata> emptyTemplate = new List<ExportColumnMetadata>();
            return Task.FromResult(emptyTemplate);
        }
    }
}
