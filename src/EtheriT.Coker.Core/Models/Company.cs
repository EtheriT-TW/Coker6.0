using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Company : FullAuditedEntity
    {
        // 原本沒有長度＝nvarchar(max)，不能建唯一索引
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(10)]
        public string TaxID { get; set; }
        [StringLength(100)]
        public string Contact { get; set; }
        [StringLength(150)]
        public string Email { get; set; }
        [StringLength(150)]
        public string Address { get; set; }
        public List<MappingCompanyAndWebsites> Websites { get; set; }

        // ── 以下為客戶管理平台（Web.Platform）專用，後台沒有對應欄位 ──
        [StringLength(50)]
        public string? Phone { get; set; }
        [StringLength(500)]
        public string? InvoiceInfo { get; set; }
        [StringLength(50)]
        public string? SalesOwner { get; set; }
        public PlatformCustomerTypeEnum CustomerType { get; set; }
        [StringLength(50)]
        public string? CustomerTypeOther { get; set; }
        [StringLength(100)]
        public string? ContactJobTitle { get; set; }
        [StringLength(50)]
        public string? ContactPhone { get; set; }
        [StringLength(150)]
        public string? ContactEmail { get; set; }
        public List<SecondaryContact> SecondaryContacts { get; set; } = [];
    }
}