using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
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
        [MaxLength(200)]
        public string UserName { get; set; } = string.Empty;
        [MaxLength(250)]
        public string Email {  get; set; }
        [MaxLength(500)]
        public string TargetEmail { get; set; }
        public string Reply {  get; set; } = string.Empty;
        public DateTime? ReplyTime {  get; set; }
        public ContactStatusEnum Status {  get; set; } = ContactStatusEnum.未處理;
        public string? FromDate { get; set; }
        public HtmlSanitizeSourceType? SourceType { get; set; }
        public long? FK_SourceId { get; set; }
        public WebMenu WebMenu { get; set; }
    }
}
