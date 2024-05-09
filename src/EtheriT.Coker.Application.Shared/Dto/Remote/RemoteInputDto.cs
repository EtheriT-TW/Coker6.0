using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Remote
{
	public class RemoteInputDto
	{
		public long FK_WebsiteId { get; set; }
		public long? FK_UserId { get; set; }
		public long FK_WebmenuId { get; set; }
		public long? FK_ArticleId { get; set; }
		public long? FK_ProdId { get; set; }
        public long? FK_TechCertId { get; set; }
    }
}
