using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public sealed class ProductListBase
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public bool Visible { get; set; }
        public bool RemovedFromShelves { get; set; }
        public int Ser_No { get; set; }
        public string? ItemNo { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsSelected { get; set; }
    }
}
