using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class FileUpload : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public Guid GuidKey { get; set; }
        [StringLength(200)] public string ContentType { get; set; }
        [StringLength(200)] public string OriginalFileName { get; set; }
        [StringLength(200)] public string? DownloadFileName { get; set; }
        public long Size { get; set; }
        public Guid? FileGuid { get; set; }
        public Website Website { get; set; }
        public List<FileBind>? fileBinds { get; set; }
        public bool IsEncryption { get; set; } = false;
        public string? AreaKey { get; set; }
    }
}
