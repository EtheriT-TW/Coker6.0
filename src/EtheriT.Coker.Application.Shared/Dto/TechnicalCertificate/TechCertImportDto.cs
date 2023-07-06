using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
	public class TechCertImportDto
	{
		public string ProdName { get; set; } = "";
		public string Title { get; set; } = "";
		public string Image1 { get; set; } = "";
		public string Description { get; set; } = "";
		public int Ser_no { get; set; } = 500;

	}
}
