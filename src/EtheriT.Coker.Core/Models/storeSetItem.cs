using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class storeSetItem : FullAuditedEntity
    {
        [StringLength(50)]
        public string Key { get; set; }
        [StringLength(150)]
        public string Value {  get; set; }
        public long FK_StoreSetId {  get; set; }
        public bool IsDefault { get; set; }
        public WebsiteLevelEnum? Level { get; set; }
        public StoreSet storeSet { get; set; }
    }
}
