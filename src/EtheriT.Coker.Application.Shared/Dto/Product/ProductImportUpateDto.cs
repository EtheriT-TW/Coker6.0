using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProductImportUpateDto
	{
		public string ProdName { get; set; } = string.Empty;
		public string Status {  get; set; } = "一般";
        public string ItemNo { get; set; } = string.Empty;
        public string SubItemNo { get; set; } = string.Empty;
        public string? Description { get; set; }
		public string? Introduction { get; set; }
		public string? Html { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public string? Spec1Name { get; set; }
		public string? Spec1 { get; set; }
		public string? Spec2Name { get; set; }
		public string? Spec2 { get; set; }
		public int Stock { get; set; } = 0;
		public double Price { get; set; } = 0;
		public string? Image1 { get; set; }
		public string? Image2 { get; set; }
		public string? Image3 { get; set; }
		public string? Image4 { get; set; }
		public string? Image5 { get; set; }
        public string? Image6 { get; set; }
        public string? Image7 { get; set; }
        public string? File1 { get; set; }
		public string? File2 { get; set; }
		public string? File3 { get; set; }
		public string? File4 { get; set; }
        public string? File5 { get; set; }
        public string? File6 { get; set; }
        public string? File7 { get; set; }
        public string? FileName1 { get; set; }
        public string? FileName2 { get; set; }
        public string? FileName3 { get; set; }
        public string? FileName4 { get; set; }
        public string? FileName5 { get; set; }
        public string? FileName6 { get; set; }
        public string? FileName7 { get; set; }
        public string? Tag1 { get; set; }
		public string? Tag2 { get; set; }
		public string? Tag3 { get; set; }
		public string? Tag4 { get; set; }
		public string? Tag5 { get; set; }
        public string? Tag6 { get; set; }
        public long? FK_WebsiteId { get; set; }
		public List<ProductStockDto>? stocks { get; set; } = new List<ProductStockDto>();
		public List<TechCertDto>? Techs { get; set; } = new List<TechCertDto>();
	}
}
