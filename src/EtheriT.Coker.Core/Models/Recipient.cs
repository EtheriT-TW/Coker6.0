using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Recipient : FullAuditedEntity
    {
        public Guid UUID { get; set; }
        [StringLength(150)]
        public string Name { get; set; }
        [StringLength(150)]
        public string Email { get; set; }
		[StringLength(300)]
		public string Address {  get; set; }
		[StringLength(10)]
		public string? ZipCode { get; set; }
		[StringLength(16)]
		public string CellPhone { get; set; }
		[StringLength(30)]
		public string TelePhone { get; set; }
        public SexEnum Sex { get; set; }
		public ShippingTypeEnum? LogisticsType { get; set; }
		[StringLength(20)]
		public string? CVSStoreID { get; set; }
		[StringLength(150)]
		public string? CVSStoreName { get; set; }
		[StringLength(300)]
		public string? CVSAddress { get; set; }
		[StringLength(30)]
		public string? CVSTelephone { get; set; }
		[StringLength(10)]
		public string? CVSOutSide { get; set; }
		public long FK_WebsiteId { get; set; }
        public Website Website { get; set; }
    }
}
