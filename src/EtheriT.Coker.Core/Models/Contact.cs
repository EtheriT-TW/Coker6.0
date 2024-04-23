using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Contact : FullAuditedEntity
    {
        public long FK_WebMenuId { get; set; }
        public string Html {  get; set; }
        [MaxLength(200)]
        public string Name {  get; set; }
        [MaxLength(250)]
        public string Email {  get; set; }
        [MaxLength(500)]
        public string TargetEmail { get; set; }
        public string Reply {  get; set; } = string.Empty;
        public DateTime? ReplyTime {  get; set; }
        public WebMenu WebMenu { get; set; }
    }
}
