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
		public string Title { get; set; }
		public string? ItemNo { get; set; }
		public string? Description { get; set; }
		public string? Introduction { get; set; }
		public string? Html { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string? Spec1Name { get; set; }
		public string? Spec1 { get; set; }
		public string? Spec2Name { get; set; }
		public string? Spec2 { get; set; }
		public int Stock { get; set; } = 0;
		public double Price { get; set; }
		public string? Image1 { get; set; }
		public string? Image2 { get; set; }
		public string? Image3 { get; set; }
		public string? Image4 { get; set; }
		public string? Image5 { get; set; }
		public string? Tech1 { get; set; }
		public string? Tech2 { get; set; }
		public string? Tech3 { get; set; }
		public string? Tech4 { get; set; }
		public string? Tag1 { get; set; }
		public string? Tag2 { get; set; }
		public string? Tag3 { get; set; }
		public string? Tag4 { get; set; }
		public string? Tag5 { get; set; }
		public long? FK_WebsiteId { get; set; }
		public List<ProductStockDto>? stocks { get; set; }
		public List<TechCertDto>? Techs { get; set; }
	}
}
