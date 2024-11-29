using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
	public class Remote
	{
		public long Id { get; set; }
		public Guid UUID { get; set; }
		public long FK_WebsiteId { get; set; }
		public long? FK_UserId { get; set; }
		public long FK_WebmenuId { get; set; }
		public long? FK_ArticleId { get; set; }
		public long? FK_ProdId { get; set; }
        public long? FK_TechCertId { get; set; }
        public DateTime ExecutionTime { get; set; }
		public DateTime LeaveTime { get; set; }
		public DateTime LastStatComputedAt { get; set; }
		public RemoteStateEnum State { get; set; }
		public int TimeOnPage { get; set; }
        [StringLength(64)]
		public string? ClientIpAddress { get; set; }
		[StringLength(512)] 
		public string? BrowserInfo { get; set; }
		public User? User { get; set; }
		public WebMenu WebMenu { get; set; }
		public Article? Article { get; set; }
		public TechnicalCertificate? TechnicalCertificate { get; set; }
        public Prod? Prod { get; set; }
	}
}
