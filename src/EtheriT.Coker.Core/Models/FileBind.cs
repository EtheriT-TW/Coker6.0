using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class FileBind : FullAuditedEntity
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public long Sid { get; set; }
        public int type { get; set; }
        public int num { get; set; }
        public int SerNo { get; set; }
        public string MediaLink { get; set; }
        public long? FK_FileUploadId { get; set; }
        public FileUpload? fileUpload { get; set; }
    }
}
