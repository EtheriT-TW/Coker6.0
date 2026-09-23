using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 記錄一次檔案回收所移除的精確 FileBind，避免還原時誤復活歷史關聯。
    /// </summary>
    public class FileRecycleBinBinding : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_FileUploadId { get; set; }
        public Guid FK_FileBindGuid { get; set; }
    }
}
