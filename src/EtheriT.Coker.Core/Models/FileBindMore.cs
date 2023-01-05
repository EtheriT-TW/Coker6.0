using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class FileBindMore
    {
        public long Id { get; set; }
        public Guid FK_FileBindGuid { get; set; }
        public long? FK_FileUploadId { get; set; }
    }
}
