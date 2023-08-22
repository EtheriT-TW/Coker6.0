using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models
{
    [Table("FileUploads")]
    public class FileUpload : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public Guid GuidKey { get; set; }
        [StringLength(50)] public string ContentType { get; set; }
        [StringLength(200)] public string OriginalFileName { get; set; }
        [StringLength(200)] public string? DownloadFileName { get; set; }
        public long Size { get; set; }
        public Guid? FileGuid { get; set; }
    }
}
