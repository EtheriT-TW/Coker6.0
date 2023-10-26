using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.AuditLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class AuditLogController : Controller
	{
		private readonly IAuditLogAppService auditLogAppService;
		public AuditLogController(IAuditLogAppService auditLogAppService) { 
			this.auditLogAppService = auditLogAppService;
		}
		[HttpGet]
		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
		{
			return await auditLogAppService.GetAllList(loadOptions);
		}
	}
}
