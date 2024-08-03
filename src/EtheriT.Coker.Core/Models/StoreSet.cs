using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class StoreSet : FullAuditedEntity
    {
        [StringLength(50)]
        public string key { get; set; }
        [StringLength(150)]
        public string name { get; set; }
        public string? memo { get; set; }
        public int? type { get; set; }
        public long FK_StoreSetGroupId { get; set; }
        public int? maxlength { get; set; }
        public string? pattern { get; set; }
        public string jobID { get; set; }
        public List<StoreSetDetail> storeSetDetails { get; set; }
        public StoreSetGroup storeSetGroup { get; set; }
        public List<storeSetItem>? storeSetItem { get; set; }
    }
}
